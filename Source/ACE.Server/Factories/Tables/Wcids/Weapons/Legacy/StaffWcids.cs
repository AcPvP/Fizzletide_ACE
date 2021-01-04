using ACE.Server.Factories.Entity;
using ACE.Server.Factories.Enum;

using WeenieClassName = ACE.Server.Factories.Enum.WeenieClassName;

namespace ACE.Server.Factories.Tables.Wcids
{
    public static class StaffWcids
    {
        private static ChanceTable<WeenieClassName> StaffWcids_Aluvian_T1;

        private static ChanceTable<WeenieClassName> StaffWcids_Aluvian = new ChanceTable<WeenieClassName>()
        {
            ( WeenieClassName.quarterstaffnew,         0.40f ),
            ( WeenieClassName.quarterstaffacidnew,     0.15f ),
            ( WeenieClassName.quarterstaffelectricnew, 0.15f ),
            ( WeenieClassName.quarterstaffflamenew,    0.15f ),
            ( WeenieClassName.quarterstafffrostnew,    0.15f ),
        };

        private static ChanceTable<WeenieClassName> StaffWcids_Gharundim_T1;

        private static ChanceTable<WeenieClassName> StaffWcids_Gharundim = new ChanceTable<WeenieClassName>()
        {
            ( WeenieClassName.nabutnew,         0.40f ),
            ( WeenieClassName.nabutacidnew,     0.15f ),
            ( WeenieClassName.nabutelectricnew, 0.15f ),
            ( WeenieClassName.nabutfirenew,     0.15f ),
            ( WeenieClassName.nabutfrostnew,    0.15f ),
        };

        private static ChanceTable<WeenieClassName> StaffWcids_Sho_T1;

        private static ChanceTable<WeenieClassName> StaffWcids_Sho = new ChanceTable<WeenieClassName>()
        {
            ( WeenieClassName.jonew,         0.40f ),
            ( WeenieClassName.joacidnew,     0.15f ),
            ( WeenieClassName.joelectricnew, 0.15f ),
            ( WeenieClassName.jofirenew,     0.15f ),
            ( WeenieClassName.jofrostnew,    0.15f ),
        };

        static StaffWcids()
        {
            if (Common.ConfigManager.Config.Server.WorldRuleset <= Common.Ruleset.Infiltration)
            {
                StaffWcids_Aluvian_T1 = new ChanceTable<WeenieClassName>()
                {
                    ( WeenieClassName.quarterstaffnew,          1.0f ),
                };

                StaffWcids_Aluvian = new ChanceTable<WeenieClassName>()
                {
                    ( WeenieClassName.quarterstaffnew,         0.80f ),
                    ( WeenieClassName.quarterstaffacidnew,     0.05f ),
                    ( WeenieClassName.quarterstaffelectricnew, 0.05f ),
                    ( WeenieClassName.quarterstaffflamenew,    0.05f ),
                    ( WeenieClassName.quarterstafffrostnew,    0.05f ),
                };

                StaffWcids_Gharundim_T1 = new ChanceTable<WeenieClassName>()
                {
                    ( WeenieClassName.nabutnew,               1.0f ),
                };

                StaffWcids_Gharundim = new ChanceTable<WeenieClassName>()
                {
                    ( WeenieClassName.nabutnew,         0.80f ),
                    ( WeenieClassName.nabutacidnew,     0.05f ),
                    ( WeenieClassName.nabutelectricnew, 0.05f ),
                    ( WeenieClassName.nabutfirenew,     0.05f ),
                    ( WeenieClassName.nabutfrostnew,    0.05f ),
                };

                StaffWcids_Sho_T1 = new ChanceTable<WeenieClassName>()
                {
                    ( WeenieClassName.jonew,               1.0f ),
                };

                StaffWcids_Sho = new ChanceTable<WeenieClassName>()
                {
                    ( WeenieClassName.jonew,         0.80f ),
                    ( WeenieClassName.joacidnew,     0.05f ),
                    ( WeenieClassName.joelectricnew, 0.05f ),
                    ( WeenieClassName.jofirenew,     0.05f ),
                    ( WeenieClassName.jofrostnew,    0.05f ),
                };
            }
        }

        public static WeenieClassName Roll(TreasureHeritageGroup heritage, int tier)
        {
            if (Common.ConfigManager.Config.Server.WorldRuleset == Common.Ruleset.EoR)
            {
                switch (heritage)
                {
                    case TreasureHeritageGroup.Aluvian:
                        return StaffWcids_Aluvian.Roll();

                    case TreasureHeritageGroup.Gharundim:
                        return StaffWcids_Gharundim.Roll();

                    case TreasureHeritageGroup.Sho:
                        return StaffWcids_Sho.Roll();
                }
            }
            else
            {
                switch (heritage)
                {
                    case TreasureHeritageGroup.Aluvian:
                        if (tier > 1)
                            return StaffWcids_Aluvian.Roll();
                        return StaffWcids_Aluvian_T1.Roll();

                    case TreasureHeritageGroup.Gharundim:
                        if (tier > 1)
                            return StaffWcids_Gharundim.Roll();
                        return StaffWcids_Gharundim_T1.Roll();

                    case TreasureHeritageGroup.Sho:
                        if (tier > 1)
                            return StaffWcids_Sho.Roll();
                        return StaffWcids_Sho_T1.Roll();
                }
            }
            return WeenieClassName.undef;
        }
    }
}
