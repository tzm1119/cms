﻿using System;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using SiteServer.Utils;
using SiteServer.BackgroundPages.Controls;
using SiteServer.BackgroundPages.Core;
using SiteServer.CMS.Caches;
using SiteServer.CMS.Database.Core;
using SiteServer.CMS.Database.Models;
using SiteServer.CMS.Fx;

namespace SiteServer.BackgroundPages.Cms
{
    public class PageTemplateLog : BasePageCms
    {
        public Repeater RptContents;
        public SqlPager SpContents;
        public Button BtnDelete;

        private int _templateId;

        public static string GetRedirectUrl(int siteId, int templateId)
        {
            return FxUtils.GetCmsUrl(siteId, nameof(PageTemplateLog), new NameValueCollection
            {
                {"TemplateID", templateId.ToString()}
            });
        }

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            _templateId = AuthRequest.GetQueryInt("templateID");

            if (AuthRequest.IsQueryExists("DeleteById"))
            {
                var arraylist = TranslateUtils.StringCollectionToIntList(Request.QueryString["IDCollection"]);
                try
                {
                    DataProvider.TemplateLog.Delete(arraylist);
                    SuccessDeleteMessage();
                }
                catch (Exception ex)
                {
                    FailDeleteMessage(ex);
                }
            }

            SpContents.ControlToPaginate = RptContents;
            SpContents.ItemsPerPage = StringUtils.Constants.PageSize;

            SpContents.SelectCommand = DataProvider.TemplateLog.GetSelectCommend(SiteId, _templateId);

            SpContents.SortField = nameof(TemplateLogInfo.Id);
            SpContents.SortMode = SortMode.DESC;
            RptContents.ItemDataBound += RptContents_ItemDataBound;

            if (IsPostBack) return;

            VerifySitePermissions(ConfigManager.WebSitePermissions.Template);

            BtnDelete.Attributes.Add("onclick",
                PageUtils.GetRedirectStringWithCheckBoxValueAndAlert(
                    FxUtils.GetCmsUrl(SiteId, nameof(PageTemplateLog), new NameValueCollection
                    {
                        {"TemplateID", _templateId.ToString()},
                        {"DeleteById", true.ToString()}
                    }), "IDCollection", "IDCollection", "请选择需要删除的修订历史！", "此操作将删除所选修订历史，确认吗？"));

            SpContents.DataBind();
        }

        private void RptContents_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var ltlIndex = (Literal)e.Item.FindControl("ltlIndex");
            var ltlAddUserName = (Literal)e.Item.FindControl("ltlAddUserName");
            var ltlAddDate = (Literal)e.Item.FindControl("ltlAddDate");
            var ltlContentLength = (Literal)e.Item.FindControl("ltlContentLength");
            var ltlView = (Literal)e.Item.FindControl("ltlView");

            var logId = SqlUtils.EvalInt(e.Item.DataItem, nameof(TemplateLogInfo.Id));

            ltlIndex.Text = Convert.ToString(e.Item.ItemIndex + 1);
            ltlAddUserName.Text = SqlUtils.EvalString(e.Item.DataItem, nameof(TemplateLogInfo.AddUserName));
            ltlAddDate.Text = DateUtils.GetDateAndTimeString(SqlUtils.EvalDateTime(e.Item.DataItem, nameof(TemplateLogInfo.AddDate)));
            ltlContentLength.Text = SqlUtils.EvalInt(e.Item.DataItem, nameof(TemplateLogInfo.ContentLength)).ToString();
            ltlView.Text =
                $@"<a href=""javascript:;"" onclick=""{ModalTemplateView.GetOpenWindowString(SiteId,
                    logId)}"">查看</a>";
        }
    }
}
