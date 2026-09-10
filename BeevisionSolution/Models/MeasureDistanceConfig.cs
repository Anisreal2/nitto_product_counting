using BeevisionSolution.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BeevisionSolution.Models
{
    internal sealed class MeasureDistanceConfig
    {
        public List<MeasureDistanceScaleSetting> DisplayScales { get; set; } = new List<MeasureDistanceScaleSetting>();

        public double GetScale(int displayId)
        {
            MeasureDistanceScaleSetting setting = DisplayScales.FirstOrDefault(
                item => item != null && item.DisplayId == displayId);
            if (setting == null || !IsValidScale(setting.ScaleMmPerPixel))
            {
                return 1.0;
            }

            return setting.ScaleMmPerPixel;
        }

        public bool SaveScale(int displayId, double scaleMmPerPixel)
        {
            if (!IsValidScale(scaleMmPerPixel))
            {
                return false;
            }

            MeasureDistanceScaleSetting setting = DisplayScales.FirstOrDefault(
                item => item != null && item.DisplayId == displayId);
            bool addedNewSetting = false;
            if (setting == null)
            {
                setting = new MeasureDistanceScaleSetting
                {
                    DisplayId = displayId
                };
                DisplayScales.Add(setting);
                addedNewSetting = true;
            }

            double previousScale = setting.ScaleMmPerPixel;
            setting.ScaleMmPerPixel = scaleMmPerPixel;

            if (SaveAtomic())
            {
                return true;
            }

            if (addedNewSetting)
            {
                DisplayScales.Remove(setting);
            }
            else
            {
                setting.ScaleMmPerPixel = previousScale;
            }
            return false;
        }

        public static MeasureDistanceConfig Load()
        {
            string configFilePath = Common.MeasureDistanceConfigFile;
            if (!File.Exists(configFilePath))
            {
                return new MeasureDistanceConfig();
            }

            try
            {
                string jsonContent = File.ReadAllText(configFilePath);
                MeasureDistanceConfig config = JsonConvert.DeserializeObject<MeasureDistanceConfig>(jsonContent);
                if (config == null)
                {
                    return new MeasureDistanceConfig();
                }

                if (config.DisplayScales == null)
                {
                    config.DisplayScales = new List<MeasureDistanceScaleSetting>();
                }

                return config;
            }
            catch (Exception ex)
            {
                Common.Bug("Cannot load Measure Distance config: {0}", ex.Message);
                return new MeasureDistanceConfig();
            }
        }

        private bool SaveAtomic()
        {
            string configFilePath = Common.MeasureDistanceConfigFile;
            string temporaryFilePath = configFilePath + ".tmp";

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));
                string jsonContent = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(temporaryFilePath, jsonContent);

                if (File.Exists(configFilePath))
                {
                    File.Replace(temporaryFilePath, configFilePath, null);
                }
                else
                {
                    File.Move(temporaryFilePath, configFilePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                Common.Bug("Cannot save Measure Distance config: {0}", ex.Message);
                try
                {
                    if (File.Exists(temporaryFilePath))
                    {
                        File.Delete(temporaryFilePath);
                    }
                }
                catch
                {
                }

                return false;
            }
        }

        private static bool IsValidScale(double scaleMmPerPixel)
        {
            if (double.IsNaN(scaleMmPerPixel) || double.IsInfinity(scaleMmPerPixel))
            {
                return false;
            }

            return scaleMmPerPixel > 0;
        }
    }

    internal sealed class MeasureDistanceScaleSetting
    {
        public int DisplayId { get; set; }
        public double ScaleMmPerPixel { get; set; } = 1.0;
    }
}
