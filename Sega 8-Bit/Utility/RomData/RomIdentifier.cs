﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BeeDevelopment.Sega8Bit.Emulation;

namespace BeeDevelopment.Sega8Bit.Utility.RomData {

	/// <summary>
	/// Provides methods for identification of ROMs.
	/// </summary>
	public class RomIdentifier {

		private List<RomDataFile> DataFiles;

		/// <summary>
		/// Creates an instance of the <see cref="RomIdentifier"/>, loading <see cref="RomDataFile"/> files from a specified directory.
		/// </summary>
		/// <param name="path">The path of the directory containing the .romdata files to load.</param>
		public RomIdentifier(string path) {
			this.DataFiles = new List<RomDataFile>();
			foreach (var DataFile in Directory.GetFiles(path, "*.romdata")) {
				this.DataFiles.Add(RomDataFile.FromFile(DataFile));
			}
		}

		/// <summary>
		/// Try and identify a <see cref="RomData"/> from its CRC-32 checksum.
		/// </summary>
		/// <param name="crc">The CRC-32 checksum to search for.</param>
		/// <param name="romData">Outputs the <see cref="RomData"/> instance found matching the supplied <paramref name="crc"/>.</param>
		/// <returns>True if a matching CRC-32 was found, false otherwise.</returns>
		public bool TryGetRomData(int crc, out RomData romData) {

			var PossibleMatches = new List<RomData>();

			foreach (var DataFile in this.DataFiles) {
				RomData PossibleMatch;
				if (DataFile.TryGetRomData(crc, out PossibleMatch)) {
					PossibleMatches.Add(PossibleMatch);
				}
			}

			PossibleMatches.Sort();

			if (PossibleMatches.Count == 0) {
				romData = default(RomData);
				return false;
			} else {
				romData = PossibleMatches[0];
				return true;
			}

		}

		/// <summary>
		/// Try and identify a <see cref="RomData"/> from a binary dump.
		/// </summary>
		/// <param name="data">The dump to identify.</param>
		/// <param name="romData">Outputs the <see cref="RomData"/> instance found matching the supplied <paramref name="data"/>.</param>
		/// <returns>True if a matching dump was found, false otherwise.</returns>
		public bool TryGetRomData(byte[] data, out RomData romData) {
			return this.TryGetRomData(Crc32.Calculate(data), out romData);
		}


		/// <summary>
		/// Guess which mapper to use.
		/// </summary>
		/// <param name="data">The data to check.</param>
		/// <returns>An <see cref="Emulator.MapperType"/> that seems the most likely one to use.</returns>
		public Emulator.MapperType GuessMapper(byte[] data) {

			if (data.Length >= 0x8000) {
				ushort CodemastersChecksumA = BitConverter.ToUInt16(data, 0x7FE6);
				ushort CodemastersChecksumB = BitConverter.ToUInt16(data, 0x7FE8);
				if (CodemastersChecksumB == 0x10000 - CodemastersChecksumA) {
					return Emulator.MapperType.Codemasters;
				}
			}

			RomData TryIdentifyHardware;
			if (TryGetRomData(data, out TryIdentifyHardware)) {
				if (TryIdentifyHardware.DataFile != null && TryIdentifyHardware.DataFile.Extensions != null) {
					var Extensions = new List<string>(Array.ConvertAll(TryIdentifyHardware.DataFile.Extensions, x=>x.ToLowerInvariant()));
					if (Extensions.Contains("sc") || Extensions.Contains("sg") || Extensions.Contains("mv") || Extensions.Contains("omv")) {
						return Emulator.MapperType.Ram;
					}
				}
			}

			return Emulator.MapperType.Standard;
		}


	}
}