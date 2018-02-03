using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace adispeak
{
    class Config
    {
        private Dictionary<string, Dictionary<string, bool>> Settings = new Dictionary<string, Dictionary<string, bool>>
        {
            {"global", new Dictionary<string, bool>
            {
            {"speech", true},
                {"sapi", false},
            {"OnChannelActionMessage", true},
            {"OnChannelCtcpMessage", true},
            {"OnChannelCtcpReplyMessage", true},
            {"OnChannelInvite", true},
            {"OnChannelJoin", true},
            {"OnChannelKick", true},
            {"OnChannelModeListBan", true},
            {"OnChannelModeListBanExempt", true},
            {"OnChannelModeListBanUnexempt", true},
            {"OnChannelModeListInviteExempt", true},
            {"OnChannelModeListInviteUnexempt", true},
            {"OnChannelModeListQuiet", true},
            {"OnChannelModeListUnban", true},
            {"OnChannelModeListUnquiet", true},
            {"OnChannelModeUserAdmined", true},
            {"OnChannelModeUserDeadmined", true},
            {"OnChannelModeUserDehalfOpped", true},
            {"OnChannelModeUserDeopped", true},
            {"OnChannelModeUserDeownered", true},
            {"OnChannelModeUserDevoiced", true},
            {"OnChannelModeUserHalfOpped", true},
            {"OnChannelModeUserOpped", true},
            {"OnChannelModeUserOwnered", true},
            {"OnChannelModeUserVoiced", true},
            {"OnChannelNormalMessage", true},
            {"OnChannelNoticeMessage", true},
            {"OnChannelPart", true},
            {"OnChannelServerModeListBan", true},
            {"OnChannelServerModeListBanExempt", true},
            {"OnChannelServerModeListBanUnexempt", true},
            {"OnChannelServerModeListInviteExempt", true},
            {"OnChannelServerModeListInviteUnexempt", true},
            {"OnChannelServerModeListQuiet", true},
            {"OnChannelServerModeListUnban", true},
            {"OnChannelServerModeListUnquiet", true},
            {"OnChannelServerModeUserAdmined", true},
            {"OnChannelServerModeUserDeadmined", true},
            {"OnChannelServerModeUserDehalfOpped", true},
            {"OnChannelServerModeUserDeopped", true},
            {"OnChannelServerModeUserDeownered", true},
            {"OnChannelServerModeUserDevoiced", true},
            {"OnChannelServerModeUserHalfOpped", true},
            {"OnChannelServerModeUserOpped", true},
            {"OnChannelServerModeUserOwnered", true},
            {"OnChannelServerModeUserVoiced", true},
            {"OnChannelTopic", true},
            {"OnConnect", true},
            {"OnConnectFailure", true},
            {"OnConnectionLogonSuccess", true},
            {"OnDisconnect", true},
            {"OnMessageSent", true},
            {"OnNick", true},
            {"OnNotifyUserOffline", true},
            {"OnNotifyUserOnline", true},
            {"OnPrivateActionMessage", true},
            {"OnPrivateCtcpMessage", true},
            {"OnPrivateNormalMessage", true},
            {"OnPrivateNoticeMessage", true},
            {"OnQuit", true},
            {"OnServerErrorMessage", true},
            {"OnServerNoticeMessage", true},
            {"OnUserInvitedToChannel", true},
            {"OnUserMode", true}
            }
            }
        };

        public bool UseSapi
        {
            get => Settings["global"]["sapi"];
            set => Settings["global"]["sapi"] = value;
        }

        public bool GetGlobal(string setting)
        {
            return Settings["global"][setting];
        }

        public void SetGlobal(string setting, bool value)
        {
            Settings["global"][setting] = value;
        }

        public bool GetWindow(string windowName, string setting)
        {
            if (Settings.ContainsKey(windowName) && Settings[windowName].ContainsKey(setting))
            {
                return Settings[windowName][setting];
            }
            else
            {
                return Settings["global"][setting];
            }
        }

        public void SetWindow(string windowName, string setting, bool value)
        {
                Settings[windowName][setting] = value;
        }

        public void AddWindow(string windowName)
        {
            Settings[windowName] = new Dictionary<string, bool>();
        }

        public bool ContainsWindow(string windowName)
        {
            return Settings.ContainsKey(windowName);
        }

        public void Read(string filename)
        {
            if (File.Exists(filename))
            {
                JsonSerializer serializer = new JsonSerializer();
                StreamReader sr = new StreamReader(filename);
                JsonTextReader reader = new JsonTextReader(sr);
                Settings = serializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(reader);
                reader.Close();
                sr.Close();
            }
        }

            public void Write(string filename)
            {
                JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
                using (StreamWriter sw = new StreamWriter(filename))
                using (JsonTextWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, Settings);
                }
            }
        }
    }
