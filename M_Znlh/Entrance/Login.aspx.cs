//
//文件名：    Login.aspx.cs
//功能描述：  登录
//创建时间：  2016/10/13
//作者：      
//修改时间：  
//修改描述：  暂无
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Leo;
using ServiceInterface.Common;

namespace M_Znlh.Entrance
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //身份校验
            if (!InterfaceTool.IdentityVerify(Request))
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, "身份认证错误！").DicInfo());
                return;
            }

            //用户名
            string strAccount = Request.Params["Account"];
            //密码
            string strPassword = Request.Params["Password"];


            AppLog log = new AppLog(Request);
            log.strAccount = strAccount;
            log.strBehavior = "用户登陆";
            log.strBehaviorURL = "/Entrance/Login.aspx";

            try
            {
                if (strAccount == null || strPassword == null)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(false, null, "参数错误，登录失败！").DicInfo());
                    return;
                }

                string strSql = null;
                
                strSql = string.Format("select username from tb_app_tally_user where logogram='{0}'", strAccount);
                var dt0 = new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally).ExecuteTable(strSql);
                if (dt0.Rows.Count >= 0)
                {
                    strSql =
                    string.Format(@"select user_id,password,serial_nam,work_no from SYSTEM_USER_TABLE
                                    where serial_nam='{0}' and password='{1}'",
                                    strAccount, strPassword);
                }
                else {
                    strSql =
                    string.Format(@"select user_id,password,serial_nam,work_no from SYSTEM_USER_TABLE
                                    where serial_nam='{0}' and password='{1}' and work_no is not null",
                                    strAccount, strPassword);
                }

                var dt1 = new Leo.SqlServer.DataAccess(RegistryKey.KeyPathTallySqlServer).ExecuteTable(strSql);
                if (dt1.Rows.Count <= 0)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(false, null, "用户名或密码错误！").DicInfo());
                    return;
                }

                //string[] strArray = new string[4];
                //strArray[0] = Convert.ToString(dt.Rows[0]["user_id"].ToString());
                //strArray[1] = Convert.ToString(dt.Rows[0]["serial_nam"].ToString());
                //strArray[2] = Convert.ToString(dt.Rows[0]["password"].ToString());
                //strArray[3] = Convert.ToString(dt.Rows[0]["work_no"].ToString());

                Json = JsonConvert.SerializeObject(new DicPackage(true, dt1.Rows[0]["user_id"].ToString(), null).DicInfo());
                log.LogCatalogSuccess();
            }
            catch (Exception ex)
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, string.Format("{0}：获取登录数据发生异常。{1}", ex.Source, ex.Message)).DicInfo());
                log.LogCatalogFailure(string.Format("{0}：获取登录数据发生异常。{1}", ex.Source, ex.Message));
            }
        }
        protected string Json;
    }
}


//strAccount = "wljz";
//strPassword = "11";
