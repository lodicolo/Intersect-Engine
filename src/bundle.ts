import { info, warning } from '@actions/core';
import { copyFile, open, stat } from 'fs/promises';
import { glob } from 'glob';
import { mkdirp } from 'mkdirp';
import { basename, dirname, join, normalize, relative, sep } from 'path';
import archiver from 'archiver';

export type Result<T = void> =
	| (T extends void ? { ok: true } : { ok: true; value: T })
	| { ok: false; err: Error };

export type FileMapping = {
	source: string;
	target: string;
};

export type DirectorySegments = string | string[];

export type BundleDescriptor = {
	directories: DirectorySegments[];
	includes: (FileMapping | string)[];
	name: string;
	platform?: string;
};

export async function makeDirectory(
	parent: string,
	segments: DirectorySegments
): Promise<Result> {
	try {
		if (typeof segments === 'string') {
			const resolvedPath = join(parent, segments);
			info(`Creating directory: ${resolvedPath}`);
			await mkdirp(resolvedPath);
			return { ok: true };
		}

		if (!Array.isArray(segments)) {
			throw new Error(`Invalid segment: ${segments}`);
		}

		const resolvedPath = join(parent, ...segments);
		info(`Creating directory: ${resolvedPath}`);
		await mkdirp(resolvedPath);
		return { ok: true };
	} catch (err) {
		return {
			ok: false,
			err: err instanceof Error ? err : new Error(err as string)
		};
	}
}

export async function packageBundle(
	version: string,
	...bundleDescriptors: BundleDescriptor[]
): Promise<Result<string[]>> {
	const repositoryRoot = process.cwd();
	info(`Bundling packages from ${repositoryRoot}`);

	const archivePaths: string[] = [];
	for (const { directories, includes, name, platform } of bundleDescriptors) {
		const bundleOutputDirectory = join('dist', name);
		await mkdirp(bundleOutputDirectory);

		for (const nestableDirectory of directories) {
			const result = await makeDirectory(
				bundleOutputDirectory,
				nestableDirectory
			);
			if (result.ok !== true) {
				return {
					ok: false,
					err: result.err
				};
			}
		}

		for (const include of includes) {
			if (typeof include === 'object') {
				const normalizedIncludeSource = normalize(include.source);
				info(
					`Searching for "${normalizedIncludeSource}" (${include.source})...`
				);
				const globbedPaths = await glob(include.source, {
					absolute: true,
					cwd: repositoryRoot,
					nodir: true,
					realpath: true
				});
				const resolvedTarget = join(
					repositoryRoot,
					bundleOutputDirectory,
					include.target
				);
				if (globbedPaths.length > 0) {
					if (globbedPaths.length > 1) {
						await mkdirp(include.target);
					}

					info(
						`Found paths for glob '${include.source}': ${JSON.stringify(
							globbedPaths,
							null,
							2
						)}`
					);

					for (const globbedFilePath of globbedPaths) {
						const globbedFileName = basename(globbedFilePath);
						info(`Resolved ${globbedFileName} (${globbedFilePath})`);
						let targetFilePath = join(resolvedTarget, globbedFileName);
						if (normalizedIncludeSource.endsWith('**')) {
							const absoluteSourceDirectory = dirname(
								join(repositoryRoot, normalizedIncludeSource)
							);
							const relativeGlobbedFilePath = relative(
								absoluteSourceDirectory,
								globbedFilePath
							);
							info(`Relative globbed file path: ${relativeGlobbedFilePath}`);
							const resolvedTargetGlobbedFilePath = join(
								resolvedTarget,
								relativeGlobbedFilePath
							);
							info(
								`Resolved target globbed file path: ${resolvedTargetGlobbedFilePath}`
							);
							if (relativeGlobbedFilePath.includes(sep)) {
								const resolvedTargetGlobbedFileDirName = dirname(
									resolvedTargetGlobbedFilePath
								);
								info(`mkdirp: ${resolvedTargetGlobbedFileDirName}`);
								await mkdirp(resolvedTargetGlobbedFileDirName);
							}
							info(
								`Re-resolved ${relativeGlobbedFilePath} to ${resolvedTargetGlobbedFilePath}`
							);
							targetFilePath = resolvedTargetGlobbedFilePath;
						} else if (globbedPaths.length === 1) {
							try {
								const stats = await stat(resolvedTarget);
								if (stats === null || !stats.isDirectory()) {
									targetFilePath = resolvedTarget;
								}
							} catch (err) {
								if (
									!err ||
									typeof err !== 'object' ||
									!('code' in err) ||
									err.code !== 'ENOENT'
								) {
									warning(err as string | Error);
								}
								targetFilePath = resolvedTarget;
							}
						}
						info(`Copying ${include.source} to ${targetFilePath}`);
						await copyFile(globbedFilePath, targetFilePath);
					}
				} else {
					info(`No files found for: ${include.source}`);
				}
			}
		}

		try {
			const archiveRootName = `intersect-${(platform ? `${platform}-` : '')}`;
			const archiveName = `${archiveRootName}-${version}.${name}.zip`;
			const archivePath = join(repositoryRoot, 'dist', archiveName);
			const fileHandle = await open(archivePath, 'w');
			const writeStream = fileHandle.createWriteStream();
			const archive = archiver('zip');

			writeStream.on('close', () => {
				info(`Wrote ${archive.pointer()}B to ${archiveName}`);
			});

			archive.on('warning', err => {
				if (err.code === 'ENOENT') {
					warning(err);
				} else {
					throw err;
				}
			});

			archive.pipe(writeStream);
			const directoryToArchive = join(repositoryRoot, 'dist', name);
			info(`Adding directory to archive: ${directoryToArchive}`);
			archive.directory(directoryToArchive, false);
			await archive.finalize();

			archivePaths.push(bundleOutputDirectory);
		} catch (err) {
			return {
				ok: false,
				err: err instanceof Error ? err : new Error(err as string)
			};
		}
	}

	return {
		ok: true,
		value: archivePaths
	};
}
