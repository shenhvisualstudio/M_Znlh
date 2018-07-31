//
//文件名：    GetShipImageOfBay.aspx.cs
//功能描述：  获取单个贝船图数据
//创建时间：  2016/11/25
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

namespace M_Znlh.Ship
{
    public partial class GetShipImageOfBay : System.Web.UI.Page
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
            //贝
            string strBayNum = Request.Params["BayNum"];
            //strShip_Id = "41";
            //strBayNum = "29";


            try
            {
                if (strShip_Id == null || strBayNum == null)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(false, null, "参数错误，获取单个贝船图数据失败！").DicInfo());
                    return;
                }

                string strSql =
                    string.Format(@"select image_id,ship_id,baynum,baycol,bayrow,container_no,size_con,container_type,code_empty,weight,work_date,sealno,moved_name,inoutmark,transmark,jjr,yb,code_crane,name 
                                    from vcon_image_monitor 
                                    WHERE ship_id='{0}' and baynum='{1}'",
                                    strShip_Id, strBayNum);
                var dt = new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally).ExecuteTable(strSql);
                if (dt.Rows.Count <= 0)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(true, null, "暂无数据！").DicInfo());
                    return;
                }

                string[,] strArray = new string[dt.Rows.Count, 18];
                for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                {
                    strArray[iRow, 0] = dt.Rows[iRow]["image_id"].ToString();
                    strArray[iRow, 1] = dt.Rows[iRow]["baynum"].ToString();
                    strArray[iRow, 2] = dt.Rows[iRow]["baycol"].ToString();
                    strArray[iRow, 3] = dt.Rows[iRow]["bayrow"].ToString();
                    strArray[iRow, 4] = dt.Rows[iRow]["container_no"].ToString();
                    strArray[iRow, 5] = dt.Rows[iRow]["size_con"].ToString();
                    strArray[iRow, 6] = dt.Rows[iRow]["container_type"].ToString();
                    strArray[iRow, 7] = dt.Rows[iRow]["code_empty"].ToString();
                    strArray[iRow, 8] = dt.Rows[iRow]["weight"].ToString();
                    strArray[iRow, 9] = dt.Rows[iRow]["work_date"].ToString();
                    strArray[iRow, 10] = dt.Rows[iRow]["sealno"].ToString();
                    strArray[iRow, 11] = dt.Rows[iRow]["moved_name"].ToString();
                    strArray[iRow, 12] = dt.Rows[iRow]["inoutmark"].ToString();
                    strArray[iRow, 13] = dt.Rows[iRow]["transmark"].ToString();
                    strArray[iRow, 14] = dt.Rows[iRow]["jjr"].ToString();
                    strArray[iRow, 15] = dt.Rows[iRow]["yb"].ToString();
                    strArray[iRow, 16] = dt.Rows[iRow]["code_crane"].ToString();
                    strArray[iRow, 17] = dt.Rows[iRow]["name"].ToString();
                }

                Json = JsonConvert.SerializeObject(new DicPackage(true, strArray, null).DicInfo());
            }
            catch (Exception ex)
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, string.Format("{0}：获取单个贝船图数据发生异常。{1}", ex.Source, ex.Message)).DicInfo());
            }
        }
        protected string Json;
    }
}