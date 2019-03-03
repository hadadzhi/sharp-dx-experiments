using SharpDX;
using System;

namespace SharpDXCommons
{
	public static class NiceFunctions
	{
		public static Color WavelengthToRGB(float wavelength, float gamma = 0.8f)
		{
			double r, g, b;

			if (wavelength >= 380 && wavelength <= 440)
			{
				double attenuation = 0.3 + 0.7 * (wavelength - 380) / (440 - 380);
				r = Math.Pow(((-(wavelength - 440) / (440 - 380)) * attenuation), gamma);
				g = 0;
				b = Math.Pow((1.0 * attenuation), gamma);
			}
			else if (wavelength >= 440 && wavelength <= 490)
			{
				r = 0;
				g = Math.Pow((wavelength - 440) / (490 - 440), gamma);
				b = 1.0;
			}
			else if (wavelength >= 490 && wavelength <= 510)
			{
				r = 0;
				g = 1.0;
				b = Math.Pow(-(wavelength - 510) / (510 - 490), gamma);
			}
			else if (wavelength >= 510 && wavelength <= 580)
			{
				r = Math.Pow((wavelength - 510) / (580 - 510), gamma);
				g = 1.0;
				b = 0;
			}
			else if (wavelength >= 580 && wavelength <= 645)
			{
				r = 1.0;
				g = Math.Pow(-(wavelength - 645) / (645 - 580), gamma);
				b = 0;
			}
			else if (wavelength >= 645 && wavelength <= 750)
			{
				double attenuation = 0.3 + 0.7 * (750 - wavelength) / (750 - 645);
				r = Math.Pow(1.0 * attenuation, gamma);
				g = 0;
				b = 0;
			}
			else
			{
				r = 0;
				g = 0;
				b = 0;
			}

			return new Color((float) r, (float) g, (float) b);
		}

		public static float Clamp(float value, float min, float max)
		{
			return value < min ? min : value > max ? max : value;
		}

		public static float Wrap(float value, float lower, float upper)
		{
			return (value % (upper - lower)) + lower;
		}
	}
}
