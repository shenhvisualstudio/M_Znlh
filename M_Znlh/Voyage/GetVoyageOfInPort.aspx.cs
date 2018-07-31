//
//文件名：    GetVoyageOfInPort.aspx.cs
//功能描述：  获取在港航次
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
using System.Data;

namespace M_Znlh.Voyage
{
    public partial class GetVoyageOfInPort : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //身份校验
            if (!InterfaceTool.IdentityVerify(Request))
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, "身份认证错误！").DicInfo());
                return;
            }

            //数据起始行
            string strStartRow = Request.Params["StartRow"];
            //行数
            string strCount = Request.Params["Count"];
            //用户ID
            string strUserId = Request.Params["UserId"];


            try
            {
                if (strStartRow == null || strCount == null || strUserId == null)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(false, null, "参数错误，获取在港航次数据失败！").DicInfo());
                    return;
                }

                string strSql = null;
                var dt = new DataTable();

                strSql = string.Format("select username from tb_app_tally_user where user_id='{0}'", strUserId);
                dt = new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally).ExecuteTable(strSql);
                if (dt.Rows.Count > 0) {
                    strSql =
                        string.Format(@"select ID SHIP_NEWID,V_ID,CODE_STATU,INOUTPORT,VOYAGE,CHI_VESSEL,ENG_VESSEL,TRADE,INOUT,WHEEL,VESSEL_CODE,VESSEL_IMO,SHIPAGENT_CHA,BERTHNO,GOODS 
                                        from VDD_SHIP_SHOW 
                                        WHERE CODE_STATU='2'
                                        ORDER BY SHIP_NEWID DESC");
                    dt = new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally).ExecuteTable(strSql, Convert.ToInt32(strStartRow) - 1, Convert.ToInt32(strStartRow) + Convert.ToInt32(strCount));
                }
                else {
                    strSql =
                         string.Format(@"select distinct c.ship_newid, a.ship_id,a.v_id,a.berthno,a.voyage,a.chi_vessel,a.inoutport,a.s_trade as trade, '' as wheel 
                                         from view_download_app a, system_user_table b, sship c
                                         where a.ship_statu = 2 and a.dept_code = '26.11.12' and(a.work_no = b.work_no or a.work_tally = b.work_no) and a.ship_id = c.ship_id and c.ship_newid is not null and b.user_id='{0}'
                                         order by a.ship_id desc ", strUserId);

                    dt = new Leo.SqlServer.DataAccess(RegistryKey.KeyPathTallySqlServer).ExecuteTable(strSql, Convert.ToInt32(strStartRow) - 1, Convert.ToInt32(strStartRow) + Convert.ToInt32(strCount));

                }

                if (dt.Rows.Count <= 0)
                {
                    string strWarning = strStartRow == "1" ? "暂无数据！" : "暂无更多数据！";
                    Json = JsonConvert.SerializeObject(new DicPackage(true, null, strWarning).DicInfo());
                    return;
                }

                string[,] strArray = new string[dt.Rows.Count, 8];
                for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                {
                    strArray[iRow, 0] = dt.Rows[iRow]["SHIP_NEWID"].ToString();
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
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, string.Format("{0}：获取在港航次数据发生异常。{1}", ex.Source, ex.Message)).DicInfo());
            }
        }
        protected string Json;
    }
}


////默认为5条
//strStartRow = "1";
//strCount = "50";
//strUserId = "220";