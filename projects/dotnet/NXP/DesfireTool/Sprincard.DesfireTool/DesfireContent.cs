using SpringCard.LibCs;
using System;
using System.Collections.Generic;

namespace Sprincard.DesfireTool
{
    public class DesfireContent
    {
        public Dictionary<Byte, DesfireApplication> DesfireApplicationEntries = new Dictionary<Byte, DesfireApplication>();        

        private bool Parse(JSONObject json)
        {
            /* is there any applications defined within the input file? */
            JSONObject jsonApplications = json.Get("Applications");

            if (jsonApplications != null)
            {
                byte cpt = 0;
                foreach (var one in jsonApplications.ObjectValue)
                {
                    JSONObject job = one.Value;
                    DesfireApplication da = DesfireApplication.CreateFromJSON(job, one.Key);
                    if (da == null)
                        return false;

                    DesfireApplicationEntries.Add(cpt, da);
                    cpt++;
                }
            }
            return true;
        }

        public static DesfireContent CreateFromJSON(JSONObject json)
        {
            DesfireContent result = new DesfireContent();

            if (result.Parse(json))
                return result;
            else return null;
        }


        public static bool TryCreateFromJSON(JSONObject json, out DesfireContent result)
        {            
            try
            {
                result = CreateFromJSON(json);
                if (result == null)
                    return false;
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
