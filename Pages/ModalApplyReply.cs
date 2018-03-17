﻿using SiteServer.Plugin;
using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using SS.GovInteract.Core;
using SS.GovInteract.Model;

namespace SS.GovInteract.Pages
{
	public class ModalApplyReply : PageBase
	{
        public Literal LtlMessage;
        protected TextBox tbReply;
        public Literal ltlDepartmentName;
        public Literal ltlUserName;
        public HtmlInputFile htmlFileUrl;

        private int _channelId;
        private int _contentId;
        private IContentInfo _contentInfo;

	    public static string GetOpenWindowString(int siteId, int channelId, int contentId)
	    {
            return LayerUtils.GetOpenScript("回复办件", $"{nameof(ModalApplyReply)}.aspx?siteId={siteId}&channelId={channelId}&contentId={contentId}", 600, 450);
	    }

	    public void Page_Load(object sender, EventArgs e)
        {
            _channelId = Utils.ToInt(Request.QueryString["channelId"]);
            _contentId = Utils.ToInt(Request.QueryString["contentId"]);

            _contentInfo = Main.Instance.ContentApi.GetContentInfo(SiteId, _channelId, _contentId);

			if (!IsPostBack)
			{
                ltlDepartmentName.Text = DepartmentManager.GetDepartmentName(AuthRequest.AdminInfo.DepartmentId);
                ltlUserName.Text = AuthRequest.AdminInfo.DisplayName;
			}
		}

        public void Submit_OnClick(object sender, EventArgs e)
        {
			var isChanged = false;

            Main.ReplyDao.DeleteByContentId(SiteId, _contentInfo.Id);
            var fileUrl = UploadFile(htmlFileUrl.PostedFile);

            var replyInfo = new ReplyInfo
            {
                SiteId = SiteId,
                ChannelId = _contentInfo.ChannelId,
                ContentId = _contentInfo.Id,
                Reply = tbReply.Text,
                DepartmentId = AuthRequest.AdminInfo.DepartmentId,
                UserName = AuthRequest.AdminName,
                AddDate = DateTime.Now
            };
            Main.ReplyDao.Insert(replyInfo);

            ApplyManager.Log(SiteId, _contentInfo.ChannelId, _contentInfo.Id, ELogTypeUtils.GetValue(ELogType.Reply), AuthRequest.AdminName, AuthRequest.AdminInfo.DepartmentId);
            if (AuthRequest.AdminInfo.DepartmentId > 0)
            {
                _contentInfo.Set(ContentAttribute.DepartmentId, AuthRequest.AdminInfo.DepartmentId.ToString());
                Main.Instance.ContentApi.Update(SiteId, _contentInfo.ChannelId, _contentInfo);
            }
            _contentInfo.Set(ContentAttribute.State, EGovInteractStateUtils.GetValue(EGovInteractState.Replied));
            Main.Instance.ContentApi.Update(SiteId, _contentInfo.ChannelId, _contentInfo);

            isChanged = true;

            if (isChanged)
			{
                LayerUtils.Close(Page);
			}
		}

        private string UploadFile(HttpPostedFile myFile)
        {
            var fileUrl = string.Empty;

            //if (myFile != null && !string.IsNullOrEmpty(myFile.FileName))
            //{
            //    var filePath = myFile.FileName;
            //    var fileExtName = PathUtils.GetExtension(filePath);
            //    var localDirectoryPath = PathUtility.GetUploadDirectoryPath(PublishmentSystemInfo, fileExtName);
            //    var localFileName = PathUtility.GetUploadFileName(PublishmentSystemInfo, filePath);

            //    var localFilePath = PathUtils.Combine(localDirectoryPath, localFileName);

            //    if (!PathUtility.IsFileExtenstionAllowed(PublishmentSystemInfo, fileExtName))
            //    {
            //        return string.Empty;
            //    }
            //    if (!PathUtility.IsFileSizeAllowed(PublishmentSystemInfo, myFile.ContentLength))
            //    {
            //        return string.Empty;
            //    }

            //    myFile.SaveAs(localFilePath);
            //    FileUtility.AddWaterMark(PublishmentSystemInfo, localFilePath);

            //    fileUrl = PageUtility.GetPublishmentSystemUrlByPhysicalPath(PublishmentSystemInfo, localFilePath);
            //    fileUrl = PageUtility.GetVirtualUrl(PublishmentSystemInfo, fileUrl);
            //}

            return fileUrl;
        }
	}
}