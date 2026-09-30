/**
 *
 * \ingroup GoogleVAS
 *
 */
using SpringCard.LibCs;
using SpringCard.NfcForum.Ndef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCard.GoogleVas
{
    /**
	 * \brief Encapsulate a Google VAS message, as it is transmitted by the NFC-enabled Android mobile
	 **/
    public class GoogleVasData
    {
        static Dictionary<string, string> objectNames = new Dictionary<string, string>();
        JSONObject jsonRoot = JSONObject.CreateArray();
        JSONObject jsonRecord = null;

        /**
         * \brief Retrieve the content of the message, as a JSON object
         **/
        public JSONObject GetJsonObject()
        {
            return jsonRoot;
        }

        /**
         * \brief Retrieve the content of the message, as a JSON string
         **/
        public string GetJsonString()
        {
            return jsonRoot.ToString(JSONEncoder.Format.Indented);
        }

        private static string IssuerTypeToText(byte data)
        {
            switch (data)
            {
                case 0x00:
                    return "Unspecified";
                case 0x01:
                    return "Merchant";
                case 0x02:
                    return "Wallet";
                case 0x03:
                    return "Manufacturer";
                default:
                    return BinConvert.ToHex(data);
            }
        }

        private static string PrimitiveToText(byte[] data)
        {
            string result = BinConvert.ToHex(data, 1, data.Length - 1);

            switch (data[0])
            {
                case 0x05:
                    while (result.StartsWith("F"))
                        result = result.Substring(1);
                    break;
            }

            return result;
        }

        internal void BeginRecord()
        {
            jsonRecord = JSONObject.CreateArray();
            jsonRoot.Add(jsonRecord);
        }

        internal void EndRecord()
        {
            jsonRecord = null;
        }

        internal void ProcessServiceDataRecord(NdefObject serviceDataRecord)
        {
            string type = serviceDataRecord.TYPE;
            
            string id = "";
            if (serviceDataRecord.ID != null)
                id = StrUtils.ToStr(serviceDataRecord.ID);

            string actualId = null;
            bool hasChildren = false;
            foreach (KeyValuePair<string, string> objectName in objectNames)
            {
                if (actualId == null)
                {
                    if (objectName.Key == type)
                    {
                        actualId = type;
                        continue;
                    }
                    if (objectName.Key == id)
                    {
                        actualId = id;
                        continue;
                    }
                }
                else if (objectName.Key.StartsWith(actualId + "."))
                {
                    hasChildren = true;
                    break;
                }
            }

            if (actualId == null)
            {
                /* Not found */
                Logger.Debug("Unknown record Type={0}, ID={1}", type, id);
                return;
            }

            string name = objectNames[actualId];
            Logger.Debug("{0}", name);

            JSONObject jsonChild = JSONObject.CreateObject();
            jsonRecord.Add(jsonChild);

            if (!hasChildren)
            {
                byte[] data = serviceDataRecord.PAYLOAD;

                if (actualId == "i")
                {
                    /* This is a service issuer */
                    if (data.Length >= 2)
                    {
                        byte issuerType = data[1];
                        data = BinUtils.Concat(data[0], BinUtils.Copy(data, 2));

                        /* Issuer */
                        jsonChild.Add("IssuerId", JSONObject.CreateString(PrimitiveToText(data)));
                        jsonChild.Add("IssuerType", JSONObject.CreateString(IssuerTypeToText(issuerType)));
                    }
                    else
                    {
                        Logger.Debug("Empty service record");
                    }
                }
                else
                {
                    Logger.Debug("Unknown record Type={0}, ID={1}", serviceDataRecord.TYPE, BinConvert.ToHex(serviceDataRecord.ID));
                    Logger.Debug("\tData={0}", BinConvert.ToHex(serviceDataRecord.PAYLOAD));
                }
            }
            else
            {
                NdefObject[] children = NdefObject.Deserialize(serviceDataRecord.PAYLOAD, false);

                if (children != null)
                {
                    foreach (NdefObject child in children)
                    {
                        type = child.TYPE;
                        type = actualId + "." + type;
                        id = "";
                        if (child.ID != null)
                            id = StrUtils.ToStr(child.ID);
                        id = actualId + "." + id;

                        string actualChildId = null;
                        foreach (KeyValuePair<string, string> objectName in objectNames)
                        {
                            if (actualChildId == null)
                            {
                                if (objectName.Key == type)
                                {
                                    actualChildId = type;
                                    break;
                                }
                                if (objectName.Key == id)
                                {
                                    actualChildId = id;
                                    break;
                                }
                            }
                        }

                        if (actualChildId != null)
                        {
                            string childName = objectNames[actualChildId];
                            string childValue;

                            if (child is RtdText)
                            {
                                childValue = ((RtdText) child).Value;
                            }
                            else
                            {
                                childValue = PrimitiveToText(child.PAYLOAD);
                            }

                            jsonChild.Add(childName, JSONObject.CreateString(childValue));
                        }
                        else
                        {
                            Logger.Debug("Unknown sub-record Type={0}, ID={1}", child.TYPE, BinConvert.ToHex(child.ID));
                            Logger.Debug("\tData={0}", BinConvert.ToHex(child.PAYLOAD));
                        }
                    }
                }
            }
        }

        static GoogleVasData()
        {
            objectNames["i"] = "Issuer";

            objectNames["cus"] = "Customer";
            objectNames["cus.cid"] = "CustomerId";
            objectNames["cus.T"] = "Language";
            objectNames["cus.cpl"] = "Language";
            objectNames["cus.cut"] = "TapId";
            objectNames["cus.cud"] = "DeviceId";

            objectNames["ly"] = "Loyalty";
            objectNames["ly.oid"] = "ObjectId";
            objectNames["ly.n"] = "Number";
            objectNames["ly.T"] = "Message";
            objectNames["ly.tr1"] = "Track1";
            objectNames["ly.tr2"] = "Track2";

            objectNames["of"] = "Offer";
            objectNames["of.oid"] = "ObjectId";
            objectNames["of.n"] = "Number";
            objectNames["of.T"] = "Message";

            objectNames["gc"] = "GiftCard";
            objectNames["gc.oid"] = "ObjectId";
            objectNames["gc.n"] = "Number";
            objectNames["gc.T"] = "Message";
            objectNames["gc.p"] = "Pin";
            objectNames["gc.tr1"] = "Track1";
            objectNames["gc.tr2"] = "Track2";

            objectNames["plc"] = "Plc";
            objectNames["plc.oid"] = "ObjectId";
            objectNames["plc.n"] = "Number";
            objectNames["plc.T"] = "Text";
            objectNames["plc.ex"] = "Expiration";
            objectNames["plc.c1"] = "Cvc1";
            objectNames["plc.tr1"] = "Track1";
            objectNames["plc.tr2"] = "Track2";

            objectNames["et"] = "EventTicket";
            objectNames["et.oid"] = "ObjectId";
            objectNames["et.n"] = "Number";
            objectNames["et.T"] = "Message";

            objectNames["fl"] = "Flight";
            objectNames["fl.oid"] = "ObjectId";
            objectNames["fl.n"] = "Number";
            objectNames["fl.T"] = "Message";

            objectNames["gr"] = "Generic";
            objectNames["gr.oid"] = "ObjectId";
            objectNames["gr.n"] = "Number";
            objectNames["gr.T"] = "Message";

        }


    }
}
