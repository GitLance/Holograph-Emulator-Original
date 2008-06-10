using System;
using System.Text;
using System.Collections;

namespace Holo.Managers
{
    /// <summary>
    /// Provides information about the various user ranks/levels.
    /// </summary>
    public static class rankManager
    {
        private static Hashtable userRanks;
        /// <summary>
        /// Initializes the various user ranks.
        /// </summary>
        public static void Init()
        {
            Out.WriteLine("Intializing rank templates...");
            userRanks = new Hashtable();

            for(byte i = 1; i <= 7; i++)
                userRanks.Add(i,new userRank(i));

            Out.WriteLine("Rank templates loaded.");
        }
        /// <summary>
        /// Returns the fuserights string for a certain user rank.
        /// </summary>
        /// <param name="rankID">The ID of the user rank.</param>
        public static string fuseRights(byte rankID)
        {
            string[] fuseRights = ((userRank)userRanks[rankID]).fuseRights;
            StringBuilder strBuilder = new StringBuilder();
            
            for (int i = 0; i < fuseRights.Length; i++)
                strBuilder.Append(fuseRights[i] + Convert.ToChar(2));

            return strBuilder.ToString();
        }
        /// <summary>
        /// Returns a bool that indicates if a certain user rank contains a certain fuseright.
        /// </summary>
        /// <param name="rankID">The ID of the user rank.</param>
        /// <param name="fuseRight">The fuseright to look for.</param>
        /// <returns></returns>
        public static bool containsRight(byte rankID, string fuseRight)
        {
            userRank objRank = ((userRank)userRanks[rankID]);
            for (int i = 0; i < objRank.fuseRights.Length; i++)
                if (objRank.fuseRights[i] == fuseRight)
                    return true;

            return false;
        }
        /// <summary>
        /// This struct represents a user rank.
        /// </summary>
        private struct userRank
        {
            internal string[] fuseRights;
            /// <summary>
            /// Initializes a user rank.
            /// </summary>
            /// <param name="rankID">The ID of the rank to initialize.</param>
            internal userRank(byte rankID)
            {
                fuseRights = DB.runReadColumn("SELECT fuseright FROM rank_fuserights WHERE minrank <= " + rankID + "", 0);
            }
        }
    }
}
