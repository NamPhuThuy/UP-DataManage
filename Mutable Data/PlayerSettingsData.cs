using System;

namespace NamPhuThuy.DataManage
{
    
    [Serializable]
    public partial class PlayerSettingsData
    {
        public float musicVolume;
        public float sfxVolume;

        public bool musicEnabled;
        public bool sfxEnabled;
        public bool vibrationEnabled;
        public bool notifyEnabled;

        public PlayerSettingsData(bool musicEnabled = true, bool sfxEnabled = true, bool vibrationEnabled = true, bool notifyEnabled = true)
        {
            this.musicEnabled = musicEnabled;
            this.sfxEnabled = sfxEnabled;
            this.vibrationEnabled = vibrationEnabled;
            this.notifyEnabled = notifyEnabled;
        }

        // For reflection serialization
        /*public PlayerSettingsData()
        {
            this.musicEnabled = true;
            this.sfxEnabled = true;
            this.vibrationEnabled = true;
            this.notifyEnabled = true;
        }*/
    }
}