﻿using System;
namespace BeeDevelopment.Sega8Bit.Hardware {
	public partial class ProgrammableSoundGenerator {

		private int[] CountDown;
		private int[] Levels;

		private int CycleStepper = 0;

		private static int CalculateParity(int val) {
			val ^= val >> 8;
			val ^= val >> 4;
			val ^= val >> 2;
			val ^= val >> 1;
			return val;
		}

		private ushort ShiftRegister;

		private bool NoiseTicking;

		private int LastCpuClocks = 0;

		/// <summary>
		/// Creates some sound samples.
		/// </summary>
		/// <param name="buffer">The buffer to write the sound samples to.</param>
		/// <remarks>
		/// The output samples are all 16-bit stereo at 44.1KHz.
		/// </remarks>
		public void CreateSamples(short[] buffer) {

			

			lock (this.Emulator) {
				lock (this.QueuedWrites) {
					
					int TotalCyclesExecuted = this.Emulator.TotalExecutedCycles;
					int ElapsedCycles = TotalCyclesExecuted - this.LastCpuClocks;

					bool WhiteNoise = (this.toneRegisters[3] & 0x04) != 0;

					for (int i = 0; i < buffer.Length; i += 2) {

						int CorrespondingCycle = (int)(((UInt64)i * (UInt64)ElapsedCycles) / (UInt64)buffer.Length);

						while (this.QueuedWrites.Count > 0 && (this.QueuedWrites.Peek().Key - this.LastCpuClocks) <= CorrespondingCycle) {
							this.WriteImmediate(this.QueuedWrites.Dequeue().Value);
						}

						this.CycleStepper -= this.ClockSpeed;
						while (this.CycleStepper < 0) {
							for (int c = 0; c < 4; ++c) {
								if (this.CountDown[c] <= 0) {

									if (c != 3) {

										this.Levels[c] = 1 - this.Levels[c];
										this.CountDown[c] = this.toneRegisters[c];
										if (this.CountDown[c] == 0) this.Levels[c] = 1;

									} else {

										switch (this.toneRegisters[c] & 0x3) {
											case 0x0: this.CountDown[c] = 0x10; break;
											case 0x1: this.CountDown[c] = 0x20; break;
											case 0x2: this.CountDown[c] = 0x40; break;
											case 0x3: this.CountDown[c] = this.toneRegisters[2]; break;
										}

										if (this.NoiseTicking ^= true) {
											this.ShiftRegister = (ushort)((this.ShiftRegister >> 1) | ((WhiteNoise ? ProgrammableSoundGenerator.CalculateParity(this.ShiftRegister & this.TappedBits) : this.ShiftRegister & 1) << (this.ShiftRegisterWidth - 1)));
											this.Levels[3] = this.ShiftRegister & 1;
										}

									}
								}
								--CountDown[c];
							}
							CycleStepper += 44100;
						}


						int Mixer = 0;
						for (int c = 0; c < 4; c++) {
							Mixer += Levels[c] * (0xF - this.volumeRegisters[c]);
						}

						buffer[i] = (short)(512 * Mixer);
						buffer[i + 1] = buffer[i];


					}

					while (this.QueuedWrites.Count > 0) this.WriteImmediate(this.QueuedWrites.Dequeue().Value);

					this.LastCpuClocks = TotalCyclesExecuted;

				}
			}
		}
	}
}