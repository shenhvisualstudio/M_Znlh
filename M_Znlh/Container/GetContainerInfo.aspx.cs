//
//文件名：    GetContainerInfo.aspx.cs
//功能描述：  获取集装箱信息
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
    public partial class GetContainerInfo : System.Web.UI.Page
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
            string strContainerNo = Request.Params["ContainerNo"];


            try
            {
                if (strShip_Id == null || strContainerNo == null)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(false, null, "参数错误，获取集装箱信息数据失败！").DicInfo());
                    return;
                }

                string strSql =
                    string.Format(@"select t.bayno,t.sealno,t.moved,t.code_unload_port,t.size_con,t.container_type,t.fullorempty,t.unload_mark 
                                    from con_image t 
                                    where t.ship_id='{0}' and t.container_no='{1}'",
                                    strShip_Id, strContainerNo);
                var dt = new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally).ExecuteTable(strSql);
                if (dt.Rows.Count <= 0)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(false, null, "暂无数据！").DicInfo());
                    return;
                }

                string[] strArray = new string[8];
                strArray[0] = dt.Rows[0]["bayno"].ToString();
                strArray[1] = dt.Rows[0]["sealno"].ToString();
                strArray[2] = dt.Rows[0]["moved"].ToString();
                strArray[3] = dt.Rows[0]["code_unload_port"].ToString();
                strArray[4] = dt.Rows[0]["size_con"].ToString();
                strArray[5] = dt.Rows[0]["container_type"].ToString();
                strArray[6] = dt.Rows[0]["fullorempty"].ToString();
                strArray[7] = dt.Rows[0]["unload_mark"].ToString();


                Json = JsonConvert.SerializeObject(new DicPackage(true, strArray, null).DicInfo());
            }
            catch (Exception ex)
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, string.Format("{0}：获取集装箱信息数据发生异常。{1}", ex.Source, ex.Message)).DicInfo());
            }
        }
        protected string Json;
    }
}