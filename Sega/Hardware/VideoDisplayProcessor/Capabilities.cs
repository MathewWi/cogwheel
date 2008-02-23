﻿namespace BeeDevelopment.Sega8Bit.Hardware {
	public partial class VideoDisplayProcessor {

		/// <summary>
		/// Gets or sets whether the <see cref="VideoDisplayProcessor"/> supports the Sega Master System and Game Gear extended mode 4.
		/// </summary>
		/// <remarks>The regular TMS9918A <see cref="VideoDisplayProcessor"/> (as used in the SG-1000 and SC-3000) did not support mode 4.</remarks>
		public bool SupportsMode4 { get; set; }

		/// <summary>
		/// Gets or sets whether the <see cref="VideoDisplayProcessor"/> supports line interrupts.
		/// </summary>
		public bool SupportsLineInterrupts { get; set; }

		/// <summary>
		/// Gets or sets whether the <see cref="VideoDisplayProcessor"/> supports a fixed palette.
		/// </summary>
		public bool SupportsFixedPalette { get; set; }

		/// <summary>
		/// Gets or sets whether the <see cref="VideoDisplayProcessor"/> supports a variable palette.
		/// </summary>
		public bool SupportsVariablePalette { get; set; }

		/// <summary>
		/// Gets or sets whether the device is using an extended (12-bit) palette.
		/// </summary>
		public bool ExtendedPalette { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of displayable sprites per scanline.
		/// </summary>
		public int SpritesPerScanline { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of zoomed sprites per scanline.
		/// </summary>
		public int ZoomedSpritesPerScanline { get; set; }

		/// <summary>
		/// Gets or sets whether the <see cref="VideoDisplayProcessor"/> supports the extended 224 and 240 line resolutions.
		/// </summary>
		public bool SupportsExtendedResolutions { get; set; }

		/// <summary>
		/// Gets or sets the method used to resize the display mode.
		/// </summary>
		public ResizingModes ResizingMode { get; set; }

		/// <summary>
		/// Defines the possible resizing modes.
		/// </summary>
		public enum ResizingModes {
			/// <summary>
			/// The video frames are output at their native resolution.
			/// </summary>
			Normal,
			/// <summary>
			/// The video frames are cropped to a 160x144 resolution.
			/// </summary>
			Cropped,
			/// <summary>
			/// The video frames are scaled to a 160x144 resolution.
			/// </summary>
			Scaled,
		}

		/// <summary>
		/// Sets the capabilities of the <see cref="VideoDisplayProcessor"/> based on a particular hardware version.
		/// </summary>
		/// <param name="model">The <see cref="HardwareModel"/> to base the capabilities on.</param>
		public void SetCapabilitiesByModel(HardwareModel model) {

			// Is it a Mark III or later?
			bool Mark3OrLater = !(model == HardwareModel.SG1000 || model == HardwareModel.SC3000);

			// General.
			this.SupportsMode4 = Mark3OrLater;
			this.SupportsLineInterrupts = Mark3OrLater;
			this.SupportsExtendedResolutions = Mark3OrLater && model != HardwareModel.MasterSystem; // Original SMS VDP doesn't support the extended resolutions.

			// Sprites.
			this.SpritesPerScanline = Mark3OrLater ? 8 : 4;
			this.ZoomedSpritesPerScanline = (Mark3OrLater && model != HardwareModel.MasterSystem) ? 8 : 4; // A bug in the SMS1 VDP means only the first four sprites are zoomed horizontally!

			// Colour.
			this.ExtendedPalette = model == HardwareModel.GameGear;
			this.SupportsVariablePalette = Mark3OrLater;
			this.SupportsFixedPalette = !(model == HardwareModel.GameGear || model == HardwareModel.GameGearMasterSystem); // The Game Gear VDP loses any sort of fixed TMS9918A palette.

			this.ResizingMode = (model == HardwareModel.GameGear ? ResizingModes.Cropped : (model == HardwareModel.GameGearMasterSystem ? ResizingModes.Scaled : ResizingModes.Normal));


		}

	}
}