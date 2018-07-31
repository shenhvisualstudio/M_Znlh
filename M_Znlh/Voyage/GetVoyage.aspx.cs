//
//文件名：    GetVoyage.aspx.cs
//功能描述：  获取航次
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

namespace M_Znlh.Voyage
{
    public partial class GetVoyage : System.Web.UI.Page
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
            //strShip_Id = "600";


            try
            {
                if (strShip_Id == null)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(false, null, "参数错误，获取航次数据失败！").DicInfo());
                    return;
                }

                string strSql =
                    string.Format(@"select ID SHIP_ID,V_ID,CODE_STATU,INOUTPORT,VOYAGE,CHI_VESSEL,ENG_VESSEL,TRADE,INOUT,WHEEL,VESSEL_CODE,VESSEL_IMO,SHIPAGENT_CHA,BERTHNO,GOODS 
                                    from VDD_SHIP_SHOW 
                                    WHERE ID='{0}'",
                                    strShip_Id);
                var dt = new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally).ExecuteTable(strSql);
                if (dt.Rows.Count <= 0)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(true, null, "暂无数据！").DicInfo());
                    return;
                }

                string[,] strArray = new string[dt.Rows.Count, 8];
                for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                {
                    strArray[iRow, 0] = dt.Rows[iRow]["SHIP_ID"].ToString();
                    strArray[iRow, 1] = dt.Rows[iRow]["V_ID"].ToString();
                    strArray[iRow, 2] = dt.Rows[iRow]["BERTHNO"].ToString();
                    strArray[iRow, 3] = dt.Rows[iRow]["VOYAGE"].ToString();
                    strArray[iRow, 4] = dt.Rows[iRow]["CHI_VESSEL"].ToString();
                    strArray[iRow, 5] = dt.Rows[iRow]["INOUTPORT"].ToString();
                    strArray[iRow, 6] = dt.Rows[iRow]["TRADE"].ToString();
                    strArray[iRow, 7] = dt.Rows[iRow]["WHEEL"].ToString();
                }

                Json = JsonConvert.SerializeObject(new DicPackage(true, strArray, null).DicInfo());
            }
            catch (Exception ex)
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, string.Format("{0}：获取航次录数据发生异常。{1}", ex.Source, ex.Message)).DicInfo());
            }
        }
        protected string Json;
    }
}