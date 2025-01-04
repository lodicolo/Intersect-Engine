using Intersect.Client.Editor.UX;
using Intersect.Client.Interface;
using Intersect.GameObjects;

Interface.StateChanged += (sender, state, mutableInterface) =>
{
    Console.WriteLine($"Interface state changed to {state.Name} by {sender}");
    var descriptorEditorWindow = mutableInterface.Create<DescriptorEditorWindow<UserVariableBase>>();
};

Intersect.Client.Core.Program.Main(args);