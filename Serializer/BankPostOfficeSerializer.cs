using System;
using CombinedAIS.Managers;
using UnityEngine;

namespace CombinedAIS.Serializer
{
    public class BankPostOfficeSerializer
    {
        // Some magic values to check we are line up correctly on the tuple boundaries
        private const uint uiTUPLE_START = 0xFEFEFEFE;
        private const uint uiTUPLE_END = 0xFAFAFAFA;

        private const ushort iBANK_POST_OFFICE_DATA_VERSION = 1;

        public static void SaveData(FastList<byte> Data)
        {
            // Write out metadata
            StorageData.WriteUInt16(iBANK_POST_OFFICE_DATA_VERSION, Data);
            StorageData.WriteInt32(BankPostOfficeManager.BankPostOfficeGoReason.Count, Data);

            // Write out each buffer settings
            foreach (var kvp in BankPostOfficeManager.BankPostOfficeGoReason)
            {
                // Write start tuple
                StorageData.WriteUInt32(uiTUPLE_START, Data);

                // Write actual settings
                StorageData.WriteUInt32(kvp.Key, Data);
                StorageData.WriteString(kvp.Value, Data);

                // Write end tuple
                StorageData.WriteUInt32(uiTUPLE_END, Data);
            }
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            if (Data != null && Data.Length > iIndex)
            {
                int iBankPostOfficeVersion = StorageData.ReadUInt16(Data, ref iIndex);
                Debug.Log("Global: " + iGlobalVersion + " BufferVersion: " + iBankPostOfficeVersion + " DataLength: " + Data.Length + " Index: " + iIndex);
                BankPostOfficeManager.BankPostOfficeGoReason ??= [];

                if (BankPostOfficeManager.BankPostOfficeGoReason.Count > 0)
                {
                    BankPostOfficeManager.BankPostOfficeGoReason.Clear();
                }

                int BankPostOffice_Count = StorageData.ReadInt32(Data, ref iIndex);
                for (int i = 0; i < BankPostOffice_Count; i++)
                {
                    CheckStartTuple($"Buffer({i})", iBankPostOfficeVersion, Data, ref iIndex);

                    uint citizenId = StorageData.ReadUInt32(Data, ref iIndex);

                    var reason = StorageData.ReadString(Data, ref iIndex);

                    BankPostOfficeManager.BankPostOfficeGoReason.Add(citizenId, reason);

                    //if end go to next item in the manager
                    // if not end read another number and then read the end
                    uint maybeEndTuple = StorageData.ReadUInt32(Data, ref iIndex);

                    if (maybeEndTuple != uiTUPLE_END)
                    {
                        StorageData.ReadUInt32(Data, ref iIndex);

                        CheckEndTuple($"Buffer({i})", iBankPostOfficeVersion, Data, ref iIndex);
                    }
                }
            }
        }

        private static void CheckStartTuple(string sTupleLocation, int iDataVersion, byte[] Data, ref int iIndex)
        {
            if (iDataVersion >= 1)
            {
                uint iTupleStart = StorageData.ReadUInt32(Data, ref iIndex);
                if (iTupleStart != uiTUPLE_START)
                {
                    throw new Exception($"BankPostOffice Buffer start tuple not found at: {sTupleLocation}");
                }
            }
        }

        private static void CheckEndTuple(string sTupleLocation, int iDataVersion, byte[] Data, ref int iIndex)
        {
            if (iDataVersion >= 1)
            {
                uint iTupleEnd = StorageData.ReadUInt32(Data, ref iIndex);
                if (iTupleEnd != uiTUPLE_END)
                {
                    throw new Exception($"BankPostOffice Buffer end tuple not found at: {sTupleLocation}");
                }
            }
        }

    }
}
