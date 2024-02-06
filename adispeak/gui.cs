using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace adispeak
{

    public class ConfigGui: Form
    {
        private bool locked;
        private TreeView treeView;
        private Button okButton;
        private Config conf;
        private Dictionary<string, bool> items;
        private Dictionary <string, bool> AvailableOptions = new Dictionary <string, bool>
            {
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
            };

        public ConfigGui(Config confparm)
        {
            conf = confparm;
            treeView = new TreeView();
            treeView.CheckBoxes = true;
            treeView.AfterCheck += new TreeViewEventHandler(treeView_AfterCheck);
            items = conf.GetWindowList();
            locked = true;
            foreach (var item in items)
            {
                treeView.Nodes.Add(item.Key, item.Key);
                foreach (var option in AvailableOptions)
                {
                    if (option.Key != "speech")
                    {
                        treeView.Nodes[item.Key].Nodes.Add(option.Key, option.Key);
                        treeView.Nodes[item.Key].Nodes[option.Key].Checked = conf.GetWindow(item.Key, option.Key);
                    }
                    else
                    {
                        treeView.Nodes[item.Key].Checked = conf.GetWindow(item.Key, option.Key);
                    }
                }
            }
            treeView.Dock = DockStyle.Fill;
            Controls.Add(treeView);
            locked = false;
            okButton = new Button();
            okButton.Text = "OK";
            okButton.Dock = DockStyle.Bottom;
            okButton.Click += new EventHandler(okButton_Click);
            Controls.Add(okButton);

            this.Size = new System.Drawing.Size(300, 300);
            this.Show();
        }

        private void treeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (locked == false)
            {
                ToggleNode(e.Node);
            }
        }

        private void ToggleNode(TreeNode node)
        {
            if (node.Parent != null)
            {
                locked = true;
                foreach (TreeNode tempnode in node.Parent.Nodes)
                {
                    if (tempnode.Checked == true)
                    {
                        node.Parent.Checked = true;
                    }
                }
                conf.SetWindow(node.Parent.Name, node.Name, node.Checked);
                locked = false;
            }
            else
            {
                locked = true;
                conf.SetWindow(node.Name, "speech", node.Checked);
                foreach (TreeNode tempnode in node.Nodes)
                {
                    tempnode.Checked = node.Checked;
                }
                locked = false;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            conf.Write("speech.json");
           this.Hide();
        }
    }
}