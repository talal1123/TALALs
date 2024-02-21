﻿using BitcrackRandomiser.Enums;

namespace BitcrackRandomiser.Helpers
{
    /// <summary>
    /// BitcrackRandomiser flagger class
    /// </summary>
    internal class Flagger
    {
        /// <summary>
        /// Flag/mark current HEX as scanned on the pool
        /// </summary>
        /// <param name="settings">Active settings model</param>
        /// <param name="hex">HEX to flag/mark</param>
        /// <param name="gpuIndex">GPU index</param>
        /// <param name="proofKey">Proof key (Without hashed)</param>
        /// <param name="gpuName">GPU name</param>
        /// <returns>
        /// true: flag completed - 
        /// false: flag failed
        /// </returns>
        public static bool Flag(Settings settings, string hex, int gpuIndex, string proofKey, string gpuName = "-")
        {
            // Hash all proof keys with SHA256
            string hashedProofKey = Helper.SHA256Hash(proofKey);

            // Try flag
            string walletAddress = settings.WalletAddress;
            if (settings.GPUCount > 1)
                walletAddress += "_" + gpuIndex;
            bool flagUsed = Requests.SetHex(hex, walletAddress, hashedProofKey, gpuName, settings.PrivatePool, settings.TargetPuzzle).Result;

            // Try flagging
            int flagTries = 1;
            int maxTries = 6;
            while (!flagUsed && flagTries <= maxTries)
            {
                flagUsed = Requests.SetHex(hex, settings.WalletAddress, hashedProofKey, gpuName, settings.TargetPuzzle).Result;
                Helper.WriteLine(string.Format("Flag error... Retrying... {0}/{1} [GPU{2}]", flagTries, maxTries, gpuIndex), MessageType.externalApp, gpuIndex: gpuIndex);
                Thread.Sleep(10000);
                flagTries++;
            }

            // Info
            if (flagUsed)
                Helper.WriteLine(string.Format("Range [{1}] scanned successfully at [GPU{0}]... Launching again...", gpuIndex, hex), MessageType.externalApp, gpuIndex: gpuIndex);
            else
                Helper.WriteLine(string.Format("Range [{1}] scanned with flag error at [GPU{0}]... Launching again...", gpuIndex, hex), MessageType.externalApp, gpuIndex: gpuIndex);

            return flagUsed;
        }
    }
}