using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdiIRCAPIv2.Arguments.Aliasing;
using AdiIRCAPIv2.Arguments.Channel;
using AdiIRCAPIv2.Arguments.ChannelMessages;
using AdiIRCAPIv2.Arguments.ChannelModes;
using AdiIRCAPIv2.Arguments.ChannelServerModes;
using AdiIRCAPIv2.Arguments.Connection;
using AdiIRCAPIv2.Arguments.Contextless;
using AdiIRCAPIv2.Arguments.PrivateMessages;
using AdiIRCAPIv2.Arguments.WindowInteraction;
using AdiIRCAPIv2.Interfaces;
using DavyKager;

namespace adispeak
{

    public class AdiSpeakPlugin : IPlugin
    {
        public string PluginName => "Adispeak";
        public string PluginDescription => "Makes AdiIrc speak via multiple screen readers using the tolk library.";
        public string PluginAuthor => "fastfinge";
        public string PluginVersion => "0.1";
        public string PluginEmail => "samuel@interfree.ca";
        private IPluginHost _host;
        private ITools _tools;
        private int CurPos;
        private bool SayTopic;
        private bool SayTopicSetBy;
        private Config config;
        
        public void Initialize(IPluginHost host)
        {
            _host = host;
            _tools = _host.Tools;
            
            Tolk.TrySAPI(true);
            Tolk.Load();

            config = new Config();
            config.Read("speech.json");

            if (!_host.HookCommand("/speak", SpeakCommandHandler))
            {
                _host.ActiveIWindow.OutputText("Could not create the /speak command.");
            }

            if (!_host.HookCommand("/braille", BrailleCommandHandler))
            {
                _host.ActiveIWindow.OutputText("Could not create the /braille command.");
            }

            if (!_host.HookCommand("/sout", SoutCommandHandler))
            {
                _host.ActiveIWindow.OutputText("Could not create the /sout command.");
            }

            if (!_host.HookCommand("/sapion", SapionCommandHandler))
            {
                _host.ActiveIWindow.OutputText("Could not create the /sapion command.");
            }

            if (!_host.HookCommand("/sapioff", SapioffCommandHandler))
            {
                _host.ActiveIWindow.OutputText("Could not create the /sapioff command.");
            }

            if (!_host.HookCommand("/savespeech", SavespeechCommandHandler))
            {
                _host.ActiveIWindow.OutputText("Could not create the /savespeech command.");
            }

            _host.HookIdentifier("screenreader", ScreenreaderIdentifierHandler);
            _host.HookIdentifier("speech", SpeechIdentifierHandler);
            _host.HookIdentifier("braille", BrailleIdentifierHandler);
            _host.HookIdentifier("speaking", SpeakingIdentifierHandler);

            _host.OnEditboxKeyUp += OnEditboxKeyUp;
            _host.OnChannelActionMessage += OnChannelActionMessage;
            _host.OnChannelCtcpMessage += OnChannelCtcpMessage;
            _host.OnChannelCtcpReplyMessage += OnChannelCtcpReplyMessage;
            _host.OnChannelInvite += OnChannelInvite;
            _host.OnChannelJoin += OnChannelJoin;
            _host.OnChannelKick += OnChannelKick;
            _host.OnChannelModeListBan += OnChannelModeListBan;
            _host.OnChannelModeListBanExempt += OnChannelModeListBanExempt;
            _host.OnChannelModeListBanUnexempt += OnChannelModeListBanUnexempt;
            _host.OnChannelModeListInviteExempt += OnChannelModeListInviteExempt;
            _host.OnChannelModeListInviteUnexempt += OnChannelModeListInviteUnexempt;
            _host.OnChannelModeListQuiet += OnChannelModeListQuiet;
            _host.OnChannelModeListUnban += OnChannelModeListUnban;
            _host.OnChannelModeListUnquiet += OnChannelModeListUnquiet;
            _host.OnChannelModeUserAdmined += OnChannelModeUserAdmined;
            _host.OnChannelModeUserDeadmined += OnChannelModeUserDeadmined;
            _host.OnChannelModeUserDehalfOpped += OnChannelModeUserDehalfOpped;
            _host.OnChannelModeUserDeopped += OnChannelModeUserDeopped;
            _host.OnChannelModeUserDeownered += OnChannelModeUserDeownered;
            _host.OnChannelModeUserDevoiced += OnChannelModeUserDevoiced;
            _host.OnChannelModeUserHalfOpped += OnChannelModeUserHalfOpped;
            _host.OnChannelModeUserOpped += OnChannelModeUserOpped;
            _host.OnChannelModeUserOwnered += OnChannelModeUserOwnered;
            _host.OnChannelModeUserVoiced += OnChannelModeUserVoiced;
            _host.OnChannelNormalMessage += OnChannelNormalMessage;
            _host.OnChannelNoticeMessage += OnChannelNoticeMessage;
            _host.OnChannelPart += OnChannelPart;
            _host.OnChannelServerModeListBan += OnChannelServerModeListBan;
            _host.OnChannelServerModeListBanExempt += OnChannelServerModeListBanExempt;
            _host.OnChannelServerModeListBanUnexempt += OnChannelServerModeListBanUnexempt;
            _host.OnChannelServerModeListInviteExempt += OnChannelServerModeListInviteExempt;
            _host.OnChannelServerModeListInviteUnexempt += OnChannelServerModeListInviteUnexempt;
            _host.OnChannelServerModeListQuiet += OnChannelServerModeListQuiet;
            _host.OnChannelServerModeListUnban += OnChannelServerModeListUnban;
            _host.OnChannelServerModeListUnquiet += OnChannelServerModeListUnquiet;
            _host.OnChannelServerModeUserAdmined += OnChannelServerModeUserAdmined;
            _host.OnChannelServerModeUserDeadmined += OnChannelServerModeUserDeadmined;
            _host.OnChannelServerModeUserDehalfOpped += OnChannelServerModeUserDehalfOpped;
            _host.OnChannelServerModeUserDeopped += OnChannelServerModeUserDeopped;
            _host.OnChannelServerModeUserDeownered += OnChannelServerModeUserDeownered;
            _host.OnChannelServerModeUserDevoiced += OnChannelServerModeUserDevoiced;
            _host.OnChannelServerModeUserHalfOpped += OnChannelServerModeUserHalfOpped;
            _host.OnChannelServerModeUserOpped += OnChannelServerModeUserOpped;
            _host.OnChannelServerModeUserOwnered += OnChannelServerModeUserOwnered;
            _host.OnChannelServerModeUserVoiced += OnChannelServerModeUserVoiced;
            _host.OnChannelTopic += OnChannelTopic;
            _host.OnConnect += OnConnect;
            _host.OnConnectFailure += OnConnectFailure;
            _host.OnConnectionLogonSuccess += OnConnectionLogonSuccess;
            _host.OnDisconnect += OnDisconnect;
            _host.OnMessageSent += OnMessageSent;
            _host.OnNick += OnNick;
            _host.OnNotifyUserOffline += OnNotifyUserOffline;
            _host.OnNotifyUserOnline += OnNotifyUserOnline;
            _host.OnPrivateActionMessage += OnPrivateActionMessage;
            _host.OnPrivateCtcpMessage += OnPrivateCtcpMessage;
            _host.OnPrivateNormalMessage += OnPrivateNormalMessage;
            _host.OnPrivateNoticeMessage += OnPrivateNoticeMessage;
            _host.OnQuit += OnQuit;
            _host.OnServerErrorMessage += OnServerErrorMessage;
            _host.OnServerNoticeMessage += OnServerNoticeMessage;
            _host.OnUserInvitedToChannel += OnUserInvitedToChannel;
            _host.OnUserMode += OnUserMode;
            _host.OnWindowFocusChanged += OnWindowFocusChanged;
            _host.OnRawServerEventReceived += OnRawServerEventReceived;
            _host.OnWindowOpened += OnWindowOpened;

            if (config.GetGlobal("sapi"))
            {
                Tolk.PreferSAPI(true);
            }
        }

        private void SpeakCommandHandler(RegisteredCommandArgs argument)
        {
            Tolk.Speak(argument.Command.Remove(0, 6));
        }

        private void BrailleCommandHandler(RegisteredCommandArgs argument)
        {
            Tolk.Braille(argument.Command.Remove(0, 8));
        }

        private void SoutCommandHandler(RegisteredCommandArgs argument)
        {
            Tolk.Output(argument.Command.Remove(0, 5));
        }

        private void SapionCommandHandler(RegisteredCommandArgs argument)
        {
            Tolk.PreferSAPI(true);
            Tolk.Output("SAPI on.");
            config.SetGlobal("sapi", true);
}

        private void SapioffCommandHandler(RegisteredCommandArgs argument)
        {
            Tolk.PreferSAPI(false);
            Tolk.Output("SAPI off.");
            config.SetGlobal("sapi", false);
        }

        private void SavespeechCommandHandler(RegisteredCommandArgs argument)
        {
            config.Write("speech.json");
            Tolk.Output("Settings saved.");
        }

        private void ScreenreaderIdentifierHandler(RegisteredIdentifierArgs argument)
        {
            argument.ReturnString = Tolk.DetectScreenReader();
        }

        private void SpeechIdentifierHandler(RegisteredIdentifierArgs argument)
        {
            if (Tolk.HasSpeech())
            {
                argument.ReturnString = "true";
            }
            else
            {
                argument.ReturnString = "false";
            }
        }

        private void BrailleIdentifierHandler(RegisteredIdentifierArgs argument)
        {
            if (Tolk.HasBraille())
            {
                argument.ReturnString = "true";
            }
            else
            {
                argument.ReturnString = "false";
            }
        }

        private void SpeakingIdentifierHandler(RegisteredIdentifierArgs argument)
        {
            if (Tolk.IsSpeaking())
            {
                argument.ReturnString = "true";
            }
            else
            {
                argument.ReturnString = "false";
            }
        }

        private void OnChannelActionMessage(ChannelActionMessageArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.User.Nick} {_tools.Strip(argument.Message)}");
                }
                else
                {
                    Tolk.Output($"{argument.User.Nick} on {argument.Channel.Name} {_tools.Strip(argument.Message)}");
                }
            }
        }

        private void OnEditboxKeyUp(EditboxKeyUpArgs argument)
        {
            if (argument.KeyEventArgs.KeyCode == Keys.F4)
            {
                if (config.GetGlobal("speech"))
                {
                    Tolk.Output("speech disabled.");
                    config.SetGlobal("speech", false);
                }
                else
                {
                    Tolk.Output("speech enabled.");
                    config.SetGlobal("speech", true);
                }
            }

            if (argument.KeyEventArgs.KeyCode == Keys.F5)
            {
                if (config.GetWindow(_host.ActiveIWindow.Name, "speech"))
                {
                    Tolk.Output($"speech for {_host.ActiveIWindow.Name} disabled.");
                    config.SetWindow(_host.ActiveIWindow.Name, "speech", false);
                }
                else
                {
                    Tolk.Output($"speech for {_host.ActiveIWindow.Name} enabled.");
                    config.SetWindow(_host.ActiveIWindow.Name, "speech", true);
                }
            }

            if (argument.KeyEventArgs.KeyCode == Keys.Tab)
            {
                Tolk.Output(argument.Editbox.Text);
            }

            if (argument.KeyEventArgs.Alt && argument.KeyEventArgs.Shift && argument.KeyEventArgs.KeyCode == Keys.Up)
            {
                CurPos = CurPos - 1;
                if (CurPos <= 0)
                {
                    Tolk.Output("Top.");
                    CurPos = 0;
                }
                _host.ActiveIWindow.TextView.ScrollTo(CurPos);
                Tolk.Output(_tools.Strip(_host.ActiveIWindow.TextView.GetLine(CurPos)));
                _host.ActiveIWindow.Editbox.Text = "";
            }

            if (argument.KeyEventArgs.Alt && argument.KeyEventArgs.Shift && argument.KeyEventArgs.KeyCode == Keys.Down)
            {
                CurPos = CurPos + 1;
                if (CurPos == _host.ActiveIWindow.TextView.Lines.Count)
                {
                    Tolk.Output("Bottom.");
                    CurPos = _host.ActiveIWindow.TextView.Lines.Count - 1;
                }
                _host.ActiveIWindow.TextView.ScrollTo(CurPos);
                Tolk.Output(_tools.Strip(_host.ActiveIWindow.TextView.GetLine(CurPos)));
                _host.ActiveIWindow.Editbox.Text = "";
            }

            if (argument.KeyEventArgs.Alt && argument.KeyEventArgs.Shift && argument.KeyEventArgs.KeyCode == Keys.Home)
            {
                CurPos = 0;
                _host.ActiveIWindow.TextView.ScrollHome();
                Tolk.Output("Top.");
                Tolk.Output(_tools.Strip(_host.ActiveIWindow.TextView.GetLine(CurPos)));
            }

            if (argument.KeyEventArgs.Alt && argument.KeyEventArgs.Shift && argument.KeyEventArgs.KeyCode == Keys.End)
            {
                CurPos = _host.ActiveIWindow.TextView.Lines.Count - 1;
                _host.ActiveIWindow.TextView.ScrollEnd();
                Tolk.Output("Bottom.");
                Tolk.Output(_tools.Strip(_host.ActiveIWindow.TextView.GetLine(CurPos)));
            }

            if (argument.KeyEventArgs.KeyCode == Keys.PageUp || argument.KeyEventArgs.KeyCode == Keys.PageDown)
            {
                CurPos = _host.ActiveIWindow.TextView.ScrollbarPos;
                if (CurPos >= _host.ActiveIWindow.TextView.Lines.Count)
                {
                    CurPos = _host.ActiveIWindow.TextView.Lines.Count - 1;
                }
                Tolk.Output(_tools.Strip(_host.ActiveIWindow.TextView.GetLine(CurPos)));
            }

            if (argument.KeyEventArgs.Control && argument.KeyEventArgs.Shift && argument.KeyEventArgs.KeyCode == Keys.Space)
            {
                Tolk.Output(_tools.Strip(_host.ActiveIWindow.TextView.GetLine(CurPos)));
            }

            if (argument.KeyEventArgs.Control && argument.KeyEventArgs.Shift && argument.KeyEventArgs.KeyCode == Keys.C)
            {
                Clipboard.SetText(_tools.Strip(_host.ActiveIWindow.TextView.GetLine(CurPos)));
                Tolk.Output("copied.");
            }

            if (argument.KeyEventArgs.KeyCode == Keys.F2)
            {
                Tolk.Silence();
            }

        }

        private void OnChannelCtcpMessage(ChannelCtcpMessageArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"ctcp from {argument.User.Nick}: {_tools.Strip(argument.Message)}");
                }
                else
                {
                    Tolk.Output($"ctcp from {argument.User.Nick} to {argument.Channel.Name}: {_tools.Strip(argument.Message)}");
                }
            }
        }

        private void OnChannelCtcpReplyMessage(ChannelCtcpReplyMessageArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"ctcp reply from {argument.User.Nick}: {_tools.Strip(argument.Message)}");
                }
                else
                {
                    Tolk.Output($"ctcp reply from {argument.User.Nick} to {argument.Channel.Name}: {_tools.Strip(argument.Message)}");
                }
            }
        }

        private void OnChannelInvite(ChannelInviteArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Server.Name, "speech") && config.GetWindow(argument.Server.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.ChannelName == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.User.Nick} on {argument.Server.Name} invites you to join this channel");
                }
                else
                {
                    Tolk.Output($"{argument.User.Nick} on {argument.Server.Name} invites you to join {argument.ChannelName}");
                }
            }
        }

        private void OnChannelJoin(ChannelJoinArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    if (argument.Channel.Server.Nick == argument.User.Nick)
                    {
                        Tolk.Output($"{argument.User.Nick} joins.");
                        SayTopic = true;
                        SayTopicSetBy = true;
                    }
                    else
                    {
                        Tolk.Output($"{argument.User.Nick} joins.");
                    }
                }
                else
                {
                    if (argument.Channel.Server.Nick == argument.User.Nick)
                    {
                        Tolk.Output($"{argument.User.Nick} joins {argument.Channel.Name}");
                        SayTopic = true;
                        SayTopicSetBy = true;
                    }
                    else
                    {
                        Tolk.Output($"{argument.User.Nick} joins {argument.Channel.Name}");
                    }
                }
            }
        }

        private void OnChannelKick(ChannelKickArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.KickedUser.Nick} is kicked by {argument.ByUser.Nick}: {_tools.Strip(argument.KickReason)}");
                }
                else
                {
                    Tolk.Output($"{argument.KickedUser.Nick} is kicked from {argument.Channel.Name} by {argument.ByUser.Nick}: {_tools.Strip(argument.KickReason)}");
                }
            }
        }

        private void OnChannelModeListBan(ChannelModeListBanArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} bans {argument.BanMask}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} bans {argument.BanMask} from {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelModeListBanExempt(ChannelModeListBanExemptArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} ban exempts {argument.BanMask}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} ban exempts {argument.BanMask} from {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelModeListBanUnexempt(ChannelModeListBanUnexemptArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} ban unexempts {argument.BanMask}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} ban unexempts {argument.BanMask} from {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelModeListInviteExempt(ChannelModeListInviteExemptArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} invite exempts {argument.BanMask}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} invite exempts {argument.BanMask} from {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelModeListInviteUnexempt(ChannelModeListInviteUnexemptArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} invite unexempts {argument.BanMask}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} invite unexempts {argument.BanMask} from {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelModeListQuiet(ChannelModeListQuietArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} quiet bans {argument.BanMask}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} quiet bans {argument.BanMask} from {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelModeListUnban(ChannelModeListUnbanArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                        Tolk.Output($"{argument.ByUser.Nick} unbans {argument.BanMask}");
                    }
            else
            {
                        Tolk.Output($"{argument.ByUser.Nick} unbans {argument.BanMask} from {argument.Channel.Name}");
                    }
                }
        }

        private void OnChannelModeListUnquiet(ChannelModeListUnquietArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} unquiet bans {argument.BanMask}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} unquiet bans {argument.BanMask} from {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelModeUserAdmined(ChannelModeUserAdminedArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} admins {argument.User.Nick}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} admins {argument.User.Nick} on {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelModeUserDeadmined(ChannelModeUserDeadminedArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} de-admins {argument.User.Nick}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} de-admins {argument.User.Nick} on {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelModeUserDehalfOpped(ChannelModeUserDehalfOppedArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} de-halfOpps {argument.User.Nick}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} de-halfOpps {argument.User.Nick} on {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelModeUserDeopped(ChannelModeUserDeoppedArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} de-opps {argument.User.Nick}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} de-opps {argument.User.Nick} on {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelModeUserDeownered(ChannelModeUserDeowneredArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} de-owners {argument.User.Nick}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} de-owners {argument.User.Nick} on {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelModeUserDevoiced(ChannelModeUserDevoicedArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} de-voices {argument.User.Nick}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} de-voices {argument.User.Nick} on {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelModeUserHalfOpped(ChannelModeUserHalfOppedArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} halfOpps {argument.User.Nick}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} halfOpps {argument.User.Nick} on {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelModeUserOpped(ChannelModeUserOppedArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} opps {argument.User.Nick}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} opps {argument.User.Nick} on {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelModeUserOwnered(ChannelModeUserOwneredArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} owners {argument.User.Nick}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} owners {argument.User.Nick} on {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelModeUserVoiced(ChannelModeUserVoicedArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                {
                    Tolk.Output($"{argument.ByUser.Nick} voices {argument.User.Nick}");
                }
                else
                {
                    Tolk.Output($"{argument.ByUser.Nick} voices {argument.User.Nick} on {argument.Channel.Name}");
                }
            }
        }

        private void OnChannelNormalMessage(ChannelNormalMessageArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"{argument.User.Nick} says {_tools.Strip(argument.Message)}");
                    }
                    else
                    {
                        Tolk.Output($"{argument.User.Nick} on {argument.Channel.Name} says {_tools.Strip(argument.Message)}");
                    }
                }
            }

        private void OnChannelNoticeMessage(ChannelNoticeMessageArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"{argument.User.Nick} notice: {_tools.Strip(argument.Message)}");
                    }
                    else
                    {
                        Tolk.Output($"{argument.User.Nick} on {argument.Channel.Name} notice: {_tools.Strip(argument.Message)}");
                    }
                }
            }

        private void OnChannelPart(ChannelPartArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"{argument.User.Nick} parts: {_tools.Strip(argument.PartMessage)}");
                    }
                    else
                    {
                        Tolk.Output($"{argument.User.Nick} parts {argument.Channel.Name}: {_tools.Strip(argument.PartMessage)}");
                    }
                }
            }

        private void OnChannelServerModeListBan(ChannelServerModeListBanArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server bans {argument.BanMask}");
                    }
                    else
                    {
                        Tolk.Output($"server bans {argument.BanMask} from {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeListBanExempt(ChannelServerModeListBanExemptArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server ban exempts {argument.BanMask}");
                    }
                    else
                    {
                        Tolk.Output($"server ban exempts {argument.BanMask} from {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeListBanUnexempt(ChannelServerModeListBanUnexemptArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server ban unexempts {argument.BanMask}");
                    }
                    else
                    {
                        Tolk.Output($"server ban unexempts {argument.BanMask} from {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeListInviteExempt(ChannelServerModeListInviteExemptArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server invite exempts {argument.BanMask}");
                    }
                    else
                    {
                        Tolk.Output($"server invite exempts {argument.BanMask} from {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeListInviteUnexempt(ChannelServerModeListInviteUnexemptArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server invite unexempts {argument.BanMask}");
                    }
                    else
                    {
                        Tolk.Output($"server invite unexempts {argument.BanMask} from {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeListQuiet(ChannelServerModeListQuietArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server quiet bans {argument.BanMask}");
                    }
                    else
                    {
                        Tolk.Output($"server quiet bans {argument.BanMask} from {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeListUnban(ChannelServerModeListUnbanArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server unbans {argument.BanMask}");
                    }
                    else
                    {
                        Tolk.Output($"server unbans {argument.BanMask} from {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeListUnquiet(ChannelServerModeListUnquietArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server quiet unbans {argument.BanMask}");
                    }
                    else
                    {
                        Tolk.Output($"server quiet unbans {argument.BanMask} from {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeUserAdmined(ChannelServerModeUserAdminedArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server admins {argument.User.Nick}");
                    }
                    else
                    {
                        Tolk.Output($"server admins {argument.User.Nick} on {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeUserDeadmined(ChannelServerModeUserDeadminedArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server de-admins {argument.User.Nick}");
                    }
                    else
                    {
                        Tolk.Output($"server de-admins {argument.User.Nick} on {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeUserDehalfOpped(ChannelServerModeUserDehalfOppedArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server de-halfOpps {argument.User.Nick}");
                    }
                    else
                    {
                        Tolk.Output($"server de-halfOpps {argument.User.Nick} on {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeUserDeopped(ChannelServerModeUserDeoppedArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server de-opps {argument.User.Nick}");
                    }
                    else
                    {
                        Tolk.Output($"server de-opps {argument.User.Nick} on {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeUserDeownered(ChannelServerModeUserDeowneredArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server de-owners {argument.User.Nick}");
                    }
                    else
                    {
                        Tolk.Output($"server de-owners {argument.User.Nick} on {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeUserDevoiced(ChannelServerModeUserDevoicedArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server de-voices {argument.User.Nick}");
                    }
                    else
                    {
                        Tolk.Output($"server de-voices {argument.User.Nick} on {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeUserHalfOpped(ChannelServerModeUserHalfOppedArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server halfOpps {argument.User.Nick}");
                    }
                    else
                    {
                        Tolk.Output($"server halfOpps {argument.User.Nick} on {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeUserOpped(ChannelServerModeUserOppedArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server opps {argument.User.Nick}");
                    }
                    else
                    {
                        Tolk.Output($"server opps {argument.User.Nick} on {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeUserOwnered(ChannelServerModeUserOwneredArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server owners {argument.User.Nick}");
                    }
                    else
                    {
                        Tolk.Output($"server owners {argument.User.Nick} on {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelServerModeUserVoiced(ChannelServerModeUserVoicedArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server voices {argument.User.Nick}");
                    }
                    else
                    {
                        Tolk.Output($"server voices {argument.User.Nick} on {argument.Channel.Name}");
                    }
                }
            }

        private void OnChannelTopic(ChannelTopicArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Channel.Name, "speech") && config.GetWindow(argument.Channel.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Channel.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"{argument.User.Nick} sets topic {_tools.Strip(argument.NewTopic)}");
                    }
                    else
                    {
                        Tolk.Output($"{argument.User.Nick} sets topic {_tools.Strip(argument.NewTopic)} on {argument.Channel.Name}");
                    }
                }
            }

        private void OnConnect(ConnectArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Server.Name, "speech") && config.GetWindow(argument.Server.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Server.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output("Connected");
                    }
                    else
                    {
                        Tolk.Output($"Connected to {argument.Server.Name}");
                    }
                }
            }

        private void OnConnectFailure(ConnectFailureArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Server.Name, "speech") && config.GetWindow(argument.Server.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Server.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"Failed to connect: {argument.Error}");
                    }
                    else
                    {
                        Tolk.Output($"Failed to connect to {argument.Server.Name}: {argument.Error}");
                    }
                }
            }

        private void OnConnectionLogonSuccess(ConnectionLogonSuccessArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Server.Name, "speech") && config.GetWindow(argument.Server.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Server.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output("Logged on");
                    }
                    else
                    {
                        Tolk.Output($"Logged on to {argument.Server.Name}");
                    }
                }
            }

        private void OnDisconnect(DisconnectArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Server.Name, "speech") && config.GetWindow(argument.Server.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Server.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output("disconnected");
                    }
                    else
                    {
                        Tolk.Output($"disconnected from {argument.Server.Name}");
                    }
                }
            }

        private void OnMessageSent(MessageSentArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Target, "speech") && config.GetWindow(argument.Target, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Target == _host.ActiveIWindow.Name)
                    {
                        if (argument.Command.StartsWith("/me"))
                        {
                            Tolk.Output($"{argument.Server.Nick} {_tools.Strip(argument.Message)}");
                        }
                        else
                        {
                            Tolk.Output($"{argument.Server.Nick} says {_tools.Strip(argument.Message)}");
                        }
                    }
                    else
                    {
                        if (argument.Command.StartsWith("/me"))
                        {
                            Tolk.Output($"{argument.Server.Nick} on {argument.Target} {_tools.Strip(argument.Message)}");
                        }
                        else
                        {
                            Tolk.Output($"{argument.Server.Nick} on {argument.Target} says {_tools.Strip(argument.Message)}");
                        }
                    }
                }
            }

        private void OnNick(NickArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Server.Name, "speech") && config.GetWindow(argument.Server.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Server.Name == _host.ActiveIWindow.Server.Name)
                    {
                        Tolk.Output($"{argument.User.Nick} now known as {argument.NewNick}");
                    }
                    else
                    {
                        Tolk.Output($"{argument.User.Nick} on {argument.Server.Name} now known as {argument.NewNick}");
                    }
                }
            }

            private void OnNotifyUserOffline(NotifyUserOfflineArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Server.Name, "speech") && config.GetWindow(argument.Server.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Server.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"notify: {argument.User.Nick} offline");
                    }
                    else
                    {
                        Tolk.Output($"notify: {argument.User.Nick} offline on {argument.Server.Name}");
                    }
                }
            }

        private void OnNotifyUserOnline(NotifyUserOnlineArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Server.Name, "speech") && config.GetWindow(argument.Server.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Server.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"notify: {argument.User.Nick} online at {argument.SignedOnTime.ToLongDateString()}");
                    }
                    else
                    {
                        Tolk.Output($"notify: {argument.User.Nick} online on {argument.Server.Name} at {argument.SignedOnTime.ToLongDateString()}");
                    }
                }
            }

        private void OnPrivateActionMessage(PrivateActionMessageArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.User.Nick, "speech") && config.GetWindow(argument.User.Nick, MethodBase.GetCurrentMethod().Name))
            {
                Tolk.Output($"{argument.User.Nick}@{argument.Server.Name} in private message {_tools.Strip(argument.Message)}");
                }
            }

        private void OnPrivateCtcpMessage(PrivateCtcpMessageArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.User.Nick, "speech") && config.GetWindow(argument.User.Nick, MethodBase.GetCurrentMethod().Name))
            {
                Tolk.Output($"{argument.User.Nick}@{argument.Server.Name} in ctcp message says {_tools.Strip(argument.Message)}");
                }
            }

        private void OnPrivateCtcpReplyMessage(PrivateCtcpReplyMessageArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.User.Nick, "speech") && config.GetWindow(argument.User.Nick, MethodBase.GetCurrentMethod().Name))
            {
                Tolk.Output($"{argument.User.Nick}@{argument.Server.Name} in ctcp reply says {_tools.Strip(argument.Message)}");
                }
            }

        private void OnPrivateNormalMessage(PrivateNormalMessageArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.User.Nick, "speech") && config.GetWindow(argument.User.Nick, MethodBase.GetCurrentMethod().Name))
            {
                Tolk.Output($"{argument.User.Nick}@{argument.Server.Name} in private message says {_tools.Strip(argument.Message)}");
                }
            }

        private void OnPrivateNoticeMessage(PrivateNoticeMessageArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.User.Nick, "speech") && config.GetWindow(argument.User.Nick, MethodBase.GetCurrentMethod().Name))
            {
                Tolk.Output($"{argument.User.Nick}@{argument.Server.Name} in private notice says {_tools.Strip(argument.Message)}");
                }
            }

                    private void OnQuit(QuitArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Server.Name, "speech") && config.GetWindow(argument.Server.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Server.Name == _host.ActiveIWindow.Server.Name)
                    {
                        Tolk.Output($"{argument.User.Nick} quit: {_tools.Strip(argument.QuitMessage)}");
                    }
                    else
                    {
                        Tolk.Output($"{argument.User.Nick} on {argument.Server.Name} quit: {_tools.Strip(argument.QuitMessage)}");
                    }
                }
        }

        private void OnServerErrorMessage(ServerErrorMessageArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Server.Name, "speech") && config.GetWindow(argument.Server.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Server.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server error: {argument.Message}");
                    }
                    else
                    {
                        Tolk.Output($"server error from {argument.Server.Name}: {argument.Message}");
                    }
                }
            }

                    private void OnServerNoticeMessage(ServerNoticeMessageArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Server.Name, "speech") && config.GetWindow(argument.Server.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Server.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"server notice: {_tools.Strip(argument.Message)}");
                    }
                    else
                    {
                        Tolk.Output($"server notice from {argument.Server.Name}: {_tools.Strip(argument.Message)}");
                    }
                }
            }

        private void OnUserInvitedToChannel(UserInvitedToChannelArgs argument)
        {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Server.Name, "speech") && config.GetWindow(argument.Server.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Server.Name == _host.ActiveIWindow.Server.Name)
                    {
                        Tolk.Output($"{argument.User.Nick} invites you to {argument.ChannelName}");
                    }
                    else
                    {
                        Tolk.Output($"{argument.User.Nick} on {argument.Server.Name} invites you to {argument.ChannelName}");
                    }
                    }
                }

        private void OnUserMode(UserModeArgs argument)
            {
            if (config.GetGlobal("speech") && config.GetGlobal(MethodBase.GetCurrentMethod().Name)
                && config.GetWindow(argument.Server.Name, "speech") && config.GetWindow(argument.Server.Name, MethodBase.GetCurrentMethod().Name))
            {
                if (argument.Server.Name == _host.ActiveIWindow.Name)
                    {
                        Tolk.Output($"you get {argument.Mode}");
                    }
                    else
                    {
                        Tolk.Output($"you get {argument.Mode} on {argument.Server.Name}");
                    }
                }
            }

        private void OnWindowOpened(WindowOpenArgs argument)
        {
            if (!config.ContainsWindow(argument.Window.Name))
            {
                config.AddWindow(argument.Window.Name);
            }
        }

        private void OnWindowFocusChanged(WindowFocusArgs argument)
        {
            Tolk.Output($"Entering {argument.Window.Name}.");
            if (!config.ContainsWindow(argument.Window.Name))
            {
                config.AddWindow(argument.Window.Name);
            }

            if (argument.Window.TextView.ScrollbarPos > 0 && argument.Window.TextView.ScrollbarPos < argument.Window.TextView.Lines.Count)
            {
                CurPos = argument.Window.TextView.ScrollbarPos;
            }
            else
            {
                CurPos = argument.Window.TextView.Lines.Count - 1;
            }
        }

        private void OnRawServerEventReceived(RawServerEventReceivedArgs argument)
        {
            if (argument.Numeric == "301") // User is away: Away message
            {
                string[] Param = argument.Message.Split(' ');
                string AwayMessage = String.Join(" ", Param, 2, Param.Length - 2);
                Tolk.Output($"{Param[1]} is away: {AwayMessage}");
            }

            if (argument.Numeric == "305" || argument.Numeric == "306") // You are now away/no longer marked as being away
            {
                string[] Param = argument.Message.Split(' ');
                string Response = String.Join(" ", Param);
                Tolk.Output(Response);
            }

            if (argument.Numeric == "307") // user is a registered nick
            {
                string[] Param = argument.Message.Split(' ');
                Tolk.Output($"{Param[1]} is a registered nick. ");
            }

            if (argument.Numeric == "311" || argument.Numeric == "314") // start of /whois or /whowas
            {
                string[] Param = argument.Message.Split(' ');
                string RealName = String.Join(" ", Param, 5, Param.Length - 5);
                string IsWas = argument.Numeric == "311" ? "is" : "was";
                Tolk.Output($"{Param[1]} {IsWas} {Param[1]}!{Param[2]}@{Param[3]} {Param[4]} {RealName}");
            }

            if (argument.Numeric == "312") // whois server list
            {
                string[] Param = argument.Message.Split(' ');
                string Response = String.Join(" ", Param, 3, Param.Length - 3);
                Tolk.Output($"{Param[1]} using {Param[2]} ({Response})");
            }

            if (argument.Numeric == "313") // is an irc operator
            {
                string[] Param = argument.Message.Split(' ');
                string Response = String.Join(" ", Param, 2, Param.Length - 2);
                Tolk.Output($"{Param[1]} {Response}");
            }

            if (argument.Numeric == "318" || argument.Numeric == "369") // end of /whois/whowas
            {
                string IsWas = argument.Numeric == "318" ? "whois" : "whowas";
                Tolk.Output($"End of /{IsWas} list.");
            }

            if (argument.Numeric == "330") // logged in as
            {
                string[] Param = argument.Message.Split(' ');
                Tolk.Output($"{Param[1]} is logged in as {Param[2]}.");
            }

            if (SayTopic && argument.Numeric == "332")
            {
                string[] Param = argument.Message.Split(' ');
                string Topic = string.Join(" ", Param, 2, Param.Length - 2);
                Tolk.Output($"The topic is: {_tools.Strip(Topic)}.");
                SayTopic = false;
            }

            if (SayTopicSetBy && argument.Numeric == "333")
            {
                string[] Param = argument.Message.Split(' ');
                string TopicSetBy = Param[2];
                Int64 TimeStamp = Convert.ToInt64(Param[3]);
                string TopicTime = DateTimeOffset.FromUnixTimeSeconds(TimeStamp).ToLocalTime().ToString("f");
                Tolk.Output($"Topic set by {TopicSetBy} on {TopicTime}.");
                SayTopicSetBy = false;
            }

            if (argument.Numeric == "671") // is a secure connection
            {
                string[] Param = argument.Message.Split(' ');
                Tolk.Output($"{Param[1]} is using a secure connection.");
            }
        }

        public void Dispose()
        {
            config.Write("speech.json");

            _host.UnHookIdentifier("screenreader");
            _host.UnHookIdentifier("speech");
            _host.UnHookIdentifier("braille");
            _host.UnHookIdentifier("speaking");

            _host.UnHookCommand("/sout");
            _host.UnHookCommand("/speak");
            _host.UnHookCommand("/braille");
            _host.UnHookCommand("/sapion");
            _host.UnHookCommand("/sapioff");
            _host.UnHookCommand("/savespeech");

            Tolk.Unload();
        }

    }
}