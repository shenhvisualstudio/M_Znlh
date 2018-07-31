//
//文件名：    GetContainerListOfVoyage.aspx.cs
//功能描述：  通过关键字获取匹配的集装箱号列表
//创建时间：  2017/6/20
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

namespace M_Znlh.Container
{
    public partial class GetContainerNoListByKeyword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //身份校验
            if (!InterfaceTool.IdentityVerify(Request))
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, "身份认证错误！").DicInfo());
                return;
            }

            //航次编码
            string strShip_Id = Request.Params["Ship_Id"];
            //集装箱号
            string strKeyword = Request.Params["Keyword"];
            //strShip_Id = "1221";
            //strKeyword = "57";
            

            try
            {
                if (strShip_Id == null || strKeyword == null)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(false, null, "参数错误，通过关键字获取匹配的集装箱号列表数据失败！").DicInfo());
                    return;
                }

                string strSql =
                    string.Format(@"select distinct t.container_no from con_image t where t.ship_id='{0}' and t.container_no like '%{1}'",
                                    strShip_Id, strKeyword);
                var dt = new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally).ExecuteTable(strSql);
                if (dt.Rows.Count <= 0)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(true, null, "暂无数据！").DicInfo());
                    return;
                }

                string[] strArray = new string[dt.Rows.Count];
                for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                {
                    strArray[iRow] = dt.Rows[iRow]["container_no"].ToString();
                }

                Json = JsonConvert.SerializeObject(new DicPackage(true, strArray, null).DicInfo());
            }
            catch (Exception ex)
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, string.Format("{0}：通过关键字获取匹配的集装箱号列表数据发生异常。{1}", ex.Source, ex.Message)).DicInfo());
            }
        }
        protected string Json;
    }
}