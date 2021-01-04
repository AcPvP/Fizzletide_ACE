using System.IO;
using System.Reflection;
using ACE.Server.Entity;
using Newtonsoft.Json;

namespace ACE.Server.Factories
{
    public class StarterGearFactory
    {
        private static StarterGearConfiguration _config = LoadConfigFromResource();

        private static StarterGearConfiguration LoadConfigFromResource()
        {
            StarterGearConfiguration config;
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ACE.Server.starterGear.json";
            if(Common.ConfigManager.Config.Server.WorldRuleset <= Common.Ruleset.Infiltration)
                resourceName = "ACE.Server.starterGear.infiltration.json";
            else if(Common.ConfigManager.Config.Server.WorldRuleset == Common.Ruleset.CustomDM)
                resourceName = "ACE.Server.starterGear.customDM.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string contents = reader.ReadToEnd();
                config = JsonConvert.DeserializeObject<StarterGearConfiguration>(contents);
            }

            return config;
        }

        public static StarterGearConfiguration GetStarterGearConfiguration()
        {
            return _config;
        }
    }
}
