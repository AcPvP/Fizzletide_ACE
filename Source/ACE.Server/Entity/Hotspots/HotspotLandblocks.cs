using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Server.Entity
{
    public static class HotspotLandblocks
    {

        private static Dictionary<uint, HotspotArea> _hotspotLandblocksMap;

        public static Dictionary<uint, HotspotArea> HotspotLandblocksMap
        {
            get
            {
                if (_hotspotLandblocksMap == null)
                {
                    _hotspotLandblocksMap = new Dictionary<uint, HotspotArea>();

                    //Irwin's Demise
                    var irwins = new HotspotArea();
                    irwins.MaxPlayersPerAllegiance = 5;
                    irwins.AreaLandblockIds = new uint[] { 0xF2EA };
                    _hotspotLandblocksMap.Add(0xF2EA, irwins);
                }

                return _hotspotLandblocksMap;
            }
        }

        public static bool IsHotspotLandblock(uint landblockId)
        {
            return HotspotLandblocksMap.ContainsKey(landblockId);
        }

        public static HotspotArea GetLandblockHotspotArea(uint landblockId)
        {
            if (IsHotspotLandblock(landblockId))
            {
                return HotspotLandblocksMap[landblockId];
            }

            return null;
        }
    }

    public class HotspotArea
    {
        public uint[] AreaLandblockIds;
        public uint MaxPlayersPerAllegiance;
    }
}
