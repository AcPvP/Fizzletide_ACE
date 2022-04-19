using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Server.Entity
{
    public static class PkTrophyLandblocks
    {

        private static Dictionary<uint, uint> _pkTrophyDropsByLandblock;

        public static Dictionary<uint, uint> PkTrophyDropsByLandblock
        {
            get
            {
                if (_pkTrophyDropsByLandblock == null)
                {
                    _pkTrophyDropsByLandblock = new Dictionary<uint, uint>();

                    //Irwin's Demise drops 2 trophies per pk death
                    _pkTrophyDropsByLandblock.Add(0xF2EA, 2);

                    //Potato Sanctuary drops 2 trophies per pk death
                    _pkTrophyDropsByLandblock.Add(0x0174, 2);

                    //TBD, which landblock is this?
                    _pkTrophyDropsByLandblock.Add(0x07FA, 1);
                }

                return _pkTrophyDropsByLandblock;
            }
        }

        public static bool IsStaticTrophyDropLandblock(uint landblockId)
        {
            return PkTrophyDropsByLandblock.ContainsKey(landblockId);
        }

        public static uint GetNumTrophiesToDropForLandblock(uint landblockId)
        {
            if (IsStaticTrophyDropLandblock(landblockId))
            {
                return PkTrophyDropsByLandblock[landblockId];
            }

            return 0;
        }
    }
}
