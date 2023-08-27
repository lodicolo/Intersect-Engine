﻿using Intersect.Compression;
using Intersect.Enums;
using Intersect.GameObjects.Maps;
using Intersect.Network;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapAttribute = Intersect.Enums.MapAttribute;

namespace Intersect.Server.Database.GameData.Migrations
{
    public partial class CerasVersionToleranceMigration
    {
        private static JsonSerializerSettings mJsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        private static Intersect.Network.Ceras mOldCeras;

        public static void Run(GameContext context)
        {
            var nameTypeDict = new Dictionary<string, Type>();

            nameTypeDict.Add("Intersect.GameObjects.Maps.TileArray[]", typeof(LegacyTileArray[]));

            nameTypeDict.Add("Intersect.GameObjects.Maps.MapAttribute[,]", typeof(LegacyMapAttribute[,]));
            nameTypeDict.Add("Intersect.GameObjects.Maps.MapAttribute", typeof(LegacyMapAttribute));
            nameTypeDict.Add("Intersect.GameObjects.Maps.MapBlockedAttribute", typeof(LegacyMapBlockedAttribute));
            nameTypeDict.Add("Intersect.GameObjects.Maps.MapItemAttribute", typeof(LegacyMapItemAttribute));
            nameTypeDict.Add("Intersect.GameObjects.Maps.MapZDimensionAttribute", typeof(LegacyMapZDimensionAttribute));
            nameTypeDict.Add("Intersect.GameObjects.Maps.MapNpcAvoidAttribute", typeof(LegacyMapNpcAvoidAttribute));
            nameTypeDict.Add("Intersect.GameObjects.Maps.MapWarpAttribute", typeof(LegacyMapWarpAttribute));
            nameTypeDict.Add("Intersect.GameObjects.Maps.MapSoundAttribute", typeof(LegacyMapSoundAttribute));
            nameTypeDict.Add("Intersect.GameObjects.Maps.MapResourceAttribute", typeof(LegacyMapResourceAttribute));
            nameTypeDict.Add("Intersect.GameObjects.Maps.MapAnimationAttribute", typeof(LegacyMapAnimationAttribute));
            nameTypeDict.Add("Intersect.GameObjects.Maps.MapGrappleStoneAttribute", typeof(LegacyMapGrappleStoneAttribute));
            nameTypeDict.Add("Intersect.GameObjects.Maps.MapSlideAttribute", typeof(LegacyMapSlideAttribute));

            mOldCeras = new LegacyCeras(nameTypeDict);

            UpdateMapTilesAttributesWithCerasVersionTolerance(context);
        }

        private static void UpdateMapTilesAttributesWithCerasVersionTolerance(GameContext context)
        {
            var connection = context.Database.GetDbConnection();
            connection.Open();
            var updates = new List<Tuple<object, byte[], byte[]>>();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "Select Id, Attributes, TileData from Maps;";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader["Id"];

                    #region "Convert tileData to Dictionary<string, Tile[,]> where the key is the layer name"
                    var tileData = (byte[])reader["TileData"];
                    var tileLayers = (LegacyTileArray[])mOldCeras.Decompress(tileData);

                    var newTileLayers = new Dictionary<string, Tile[,]>();
                    for (int i = 0; i < tileLayers.Length; i++)
                    {
                        var knownLayers = new string[] { "Ground", "Mask 1", "Mask 2", "Fringe 1", "Fringe 2" };
                        var name = i < knownLayers.Length ? knownLayers[i] : "Layer " + i;
                        var tiles = new Tile[tileLayers[i].Tiles.GetLength(0), tileLayers[i].Tiles.GetLength(1)];
                        for (int x = 0; x < tileLayers[i].Tiles.GetLength(0); x++)
                        {
                            for (int y = 0; y < tileLayers[i].Tiles.GetLength(1); y++)
                            {
                                tiles[x, y] = new Tile()
                                {
                                    X = tileLayers[i].Tiles[x, y].X,
                                    Y = tileLayers[i].Tiles[x, y].Y,
                                    Autotile = tileLayers[i].Tiles[x, y].Autotile,
                                    TilesetId = tileLayers[i].Tiles[x, y].TilesetId,
                                    TilesetTex = tileLayers[i].Tiles[x, y].TilesetTex
                                };
                            }
                        }
                        newTileLayers.Add(name, tiles);
                    }

                    //Ceras is too buggy for this at rest storage. Switching to compressed json for now.
                    //Using ceras to compress and then decompress newTileLayers would result (for some reason) in it losing all tileset ids
                    var newTileData = LZ4.PickleString(JsonConvert.SerializeObject(newTileLayers, Formatting.None, mJsonSerializerSettings));
                    #endregion

                    #region "Convert Attributes back into the same data type, but with version tolerance"
                    var attributeData = (byte[])reader["Attributes"];

                    //Only works on Ceras 4.0.40 -- I don't know why..
                    //Later versions of ceras are unable to decompress map attributes (on SOME maps)
                    //Feel free to ask JC for the demo game db if you want to debug
                    var attributes = mOldCeras.Decompress<LegacyMapAttribute[,]>(attributeData);

                    //Trying to use Ceras to re-serialize as the proper classes loses guid values for some reason, so lets do some fun json to get this converted.
                    var legacyAttributes = JsonConvert.SerializeObject(attributes, Formatting.None, mJsonSerializerSettings);

                    //Fix type names?
                    var attributesJson = legacyAttributes.Replace("Intersect.Server.Database.GameData.Migrations.CerasVersionToleranceMigration+Legacy", "Intersect.GameObjects.Maps.").Replace(", Intersect Server", ", Intersect Core");

                    var newAttributeDataJson = LZ4.PickleString(attributesJson);
                    #endregion

                    updates.Add(new Tuple<object, byte[], byte[]>(id, newAttributeDataJson, newTileData));
                }
            }

            connection.Close();
            connection.Open();
            if (updates.Count > 0)
            {
                var trans = connection.BeginTransaction();

                using (var updateCmd = connection.CreateCommand())
                {
                    updateCmd.CommandText = string.Empty;
                    updateCmd.Transaction = trans;
                    var i = 0;
                    var currentCount = 0;
                    foreach (var update in updates)
                    {
                        updateCmd.CommandText += "UPDATE Maps SET Attributes = @Attributes" +
                                                 i +
                                                 ", TileData = @TileData" +
                                                 i +
                                                 " WHERE Id = @Id" +
                                                 i +
                                                 ";";

                        if (context.Database.ProviderName.Contains("Sqlite"))
                        {
                            updateCmd.Parameters.Add(new SqliteParameter("@Id" + i, (object)update.Item1));
                            updateCmd.Parameters.Add(new SqliteParameter("@Attributes" + i, (byte[])update.Item2));
                            updateCmd.Parameters.Add(new SqliteParameter("@TileData" + i, (byte[])update.Item3));
                        }
                        else
                        {
                            updateCmd.Parameters.Add(new MySqlParameter("@Id" + i, (object)update.Item1));
                            updateCmd.Parameters.Add(new MySqlParameter("@Attributes" + i, (byte[])update.Item2));
                            updateCmd.Parameters.Add(new MySqlParameter("@TileData" + i, (byte[])update.Item3));
                        }

                        i++;
                        currentCount++;

                        if (currentCount > 256)
                        {
                            updateCmd.ExecuteNonQuery();
                            updateCmd.CommandText = "";
                            updateCmd.Parameters.Clear();
                            currentCount = 0;
                        }
                    }

                    updateCmd.ExecuteNonQuery();
                    updateCmd.Parameters.Clear();
                }

                trans.Commit();
            }

            connection.Close();
        }

        private partial struct LegacyTileArray
        {

            public LegacyTile[,] Tiles;

        }

        private partial struct LegacyTile
        {

            public Guid TilesetId;

            public int X;

            public int Y;

            public byte Autotile;

            [JsonIgnore] public object TilesetTex;

        }

        private abstract partial class LegacyMapAttribute
        {
            public abstract MapAttribute Type { get; }

            public static LegacyMapAttribute CreateAttribute(MapAttribute type)
            {
                switch (type)
                {
                    case MapAttribute.Walkable:
                        return null;
                    case MapAttribute.Blocked:
                        return new LegacyMapBlockedAttribute();
                    case MapAttribute.Item:
                        return new LegacyMapItemAttribute();
                    case MapAttribute.ZDimension:
                        return new LegacyMapZDimensionAttribute();
                    case MapAttribute.NpcAvoid:
                        return new LegacyMapNpcAvoidAttribute();
                    case MapAttribute.Warp:
                        return new LegacyMapWarpAttribute();
                    case MapAttribute.Sound:
                        return new LegacyMapSoundAttribute();
                    case MapAttribute.Resource:
                        return new LegacyMapResourceAttribute();
                    case MapAttribute.Animation:
                        return new LegacyMapAnimationAttribute();
                    case MapAttribute.GrappleStone:
                        return new LegacyMapGrappleStoneAttribute();
                    case MapAttribute.Slide:
                        return new LegacyMapSlideAttribute();
                }

                return null;
            }
        }

        private partial class LegacyMapBlockedAttribute : LegacyMapAttribute
        {

            public override MapAttribute Type { get; } = MapAttribute.Blocked;

        }

        private partial class LegacyMapItemAttribute : LegacyMapAttribute
        {

            public override MapAttribute Type { get; } = MapAttribute.Item;

            public Guid ItemId { get; set; }

            public int Quantity { get; set; }

        }

        private partial class LegacyMapZDimensionAttribute : LegacyMapAttribute
        {

            public override MapAttribute Type { get; } = MapAttribute.ZDimension;

            public byte GatewayTo { get; set; }

            public byte BlockedLevel { get; set; }

        }

        private partial class LegacyMapNpcAvoidAttribute : LegacyMapAttribute
        {

            public override MapAttribute Type { get; } = MapAttribute.NpcAvoid;

        }

        private partial class LegacyMapWarpAttribute : LegacyMapAttribute
        {

            public override MapAttribute Type { get; } = MapAttribute.Warp;

            public Guid MapId { get; set; }

            public byte X { get; set; }

            public byte Y { get; set; }

            public WarpDirection Direction { get; set; } = WarpDirection.Retain;

        }

        private partial class LegacyMapSoundAttribute : LegacyMapAttribute
        {

            public override MapAttribute Type { get; } = MapAttribute.Sound;

            public string File { get; set; }

            public byte Distance { get; set; }

        }

        private partial class LegacyMapResourceAttribute : LegacyMapAttribute
        {

            public override MapAttribute Type { get; } = MapAttribute.Resource;

            public Guid ResourceId { get; set; }

            public byte SpawnLevel { get; set; }

        }

        private partial class LegacyMapAnimationAttribute : LegacyMapAttribute
        {

            public override MapAttribute Type { get; } = MapAttribute.Animation;

            public Guid AnimationId { get; set; }

        }

        private partial class LegacyMapGrappleStoneAttribute : LegacyMapAttribute
        {

            public override MapAttribute Type { get; } = MapAttribute.GrappleStone;

        }

        private partial class LegacyMapSlideAttribute : LegacyMapAttribute
        {

            public override MapAttribute Type { get; } = MapAttribute.Slide;

            public byte Direction { get; set; }

        }
    }
}
