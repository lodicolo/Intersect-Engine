import {error, getInput, info, setFailed, setOutput} from '@actions/core';
import {glob} from 'glob';
import {readFile} from 'fs/promises';
import {BundleDescriptor, Result, packageBundle} from './bundle';

async function readBundle(
	bundleFilePath: string
): Promise<Result<BundleDescriptor>> {
	try {
		const bundleFileRaw = await readFile(bundleFilePath, 'utf8');
		const bundleDescriptor: BundleDescriptor = JSON.parse(bundleFileRaw);
		return {
			ok: true,
			value: bundleDescriptor
		};
	} catch (err) {
		return {
			ok: false,
			err: err instanceof Error ? err : new Error(err as string)
		};
	}
}

async function run(): Promise<void> {
	try {
		const bundleFilePath: string = getInput('bundle');
		const globbedBundleFilePaths = await glob(bundleFilePath);
		const bundleDescriptors: BundleDescriptor[] = [];

		let failed = false;
		for (const globbedBundleFilePath of globbedBundleFilePaths) {
			const result = await readBundle(globbedBundleFilePath);
			if (result.ok) {
				bundleDescriptors.push(result.value);
			} else {
				error(result.err);
				failed = true;
			}
		}

		if (failed) {
			setFailed('One or more bundle files could not be read.');
			return;
		}

		const version: string = getInput('version');

		const result = await packageBundle(version, ...bundleDescriptors);
		if (!result.ok) {
			error(result.err);
			setFailed('One or more bundles could not be packaged.');
			return;
		}

		info('Finished packaging bundles.');
		setOutput('archives', result.value);
	} catch (err) {
		setFailed(err as string | Error);
	}
}

run();
