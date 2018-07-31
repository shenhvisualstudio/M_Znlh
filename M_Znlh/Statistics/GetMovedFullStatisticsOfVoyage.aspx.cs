//
//文件名：    GetMovedFullStatisticsOfVoyage.aspx.cs
//功能描述：  获取捣箱全统计
//创建时间：  2017/3/24
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
using Leo.Oracle;

namespace M_Znlh.Statistics
{
    public partial class GetMovedFullStatisticsOfVoyage : System.Web.UI.Page
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
            //strShip_Id = "2015";


            try
            {
                if (strShip_Id == null)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(true, null, "参数错误，获取个统计理货员数据数据失败！").DicInfo());
                    return;
                }
                //strShip_Id = string.IsNullOrWhiteSpace(strShip_Id) ? string.Empty : strShip_Id;

                string strSql = string.Empty;
                string strAbnormalSql = string.Empty;
                FullStatisticsE fullStatisticsE = new FullStatisticsE();


                strSql =
                    string.Format(@"select SIZE_CON,FULLOREMPTY,count(*) allCon,count(case when unload_mark='1' then 1 end) tallyCon 
                                    from con_image t 
                                    WHERE SHIP_ID='{0}' and MOVED='1'  
                                    GROUP BY  SIZE_CON,FULLOREMPTY",
                                    strShip_Id);

                strAbnormalSql =
                    string.Format(@"select SIZE_CON,FULLOREMPTY,count(*) abnormalCon 
                                    from con_image t 
                                    WHERE SHIP_ID='{0}' and MOVED='1' and BAY_OPERTIME is not null 
                                    GROUP BY  SIZE_CON,FULLOREMPTY",
                                    strShip_Id);



                var dt = new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally).ExecuteTable(strSql);
                if (dt.Rows.Count > 0) {

                    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                    {

                        if (dt.Rows[iRow]["FULLOREMPTY"].Equals("E"))
                        {
                            if (dt.Rows[iRow]["SIZE_CON"].Equals("20"))
                            {
                                fullStatisticsE.Forecast_E_20 = Convert.ToInt16(dt.Rows[iRow]["ALLCON"]);
                                fullStatisticsE.Tally_E_20 = Convert.ToInt16(dt.Rows[iRow]["TALLYCON"]);

                            }
                            else if (dt.Rows[iRow]["SIZE_CON"].Equals("40"))
                            {
                                fullStatisticsE.Forecast_E_40 = Convert.ToInt16(dt.Rows[iRow]["ALLCON"]);
                                fullStatisticsE.Tally_E_40 = Convert.ToInt16(dt.Rows[iRow]["TALLYCON"]);
                            }
                            else
                            {
                                fullStatisticsE.Forecast_E_other = Convert.ToInt16(dt.Rows[iRow]["ALLCON"]);
                                fullStatisticsE.Tally_E_other = Convert.ToInt16(dt.Rows[iRow]["TALLYCON"]);                    
                            }
                        }
                        if (dt.Rows[iRow]["FULLOREMPTY"].Equals("F"))
                        {
                            if (dt.Rows[iRow]["SIZE_CON"].Equals("20"))
                            {
                                fullStatisticsE.Forecast_F_20 = Convert.ToInt16(dt.Rows[iRow]["ALLCON"]);
                                fullStatisticsE.Tally_F_20 = Convert.ToInt16(dt.Rows[iRow]["TALLYCON"]);
                            }
                            else if (dt.Rows[iRow]["SIZE_CON"].Equals("40"))
                            {
                                fullStatisticsE.Forecast_F_40 = Convert.ToInt16(dt.Rows[iRow]["ALLCON"]);
                                fullStatisticsE.Tally_F_40 = Convert.ToInt16(dt.Rows[iRow]["TALLYCON"]);
                            }
                            else
                            {
                                fullStatisticsE.Forecast_F_other = Convert.ToInt16(dt.Rows[iRow]["ALLCON"]);
                                fullStatisticsE.Tally_F_other = Convert.ToInt16(dt.Rows[iRow]["TALLYCON"]);
                            }
                        }
                    }

                }



                dt = new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally).ExecuteTable(strAbnormalSql);
                if (dt.Rows.Count > 0)
                {

                    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                    {

                        if (dt.Rows[iRow]["FULLOREMPTY"].Equals("E"))
                        {
                            if (dt.Rows[iRow]["SIZE_CON"].Equals("20"))
                            {
                                fullStatisticsE.Abnormal_E_20 = Convert.ToInt16(dt.Rows[iRow]["ABNORMALCON"]);

                            }
                            else if (dt.Rows[iRow]["SIZE_CON"].Equals("40"))
                            {
                                fullStatisticsE.Abnormal_E_40 = Convert.ToInt16(dt.Rows[iRow]["ABNORMALCON"]);
                            }
                            else
                            {
                                fullStatisticsE.Abnormal_E_other = Convert.ToInt16(dt.Rows[iRow]["ABNORMALCON"]);
                            }
                        }
                        if (dt.Rows[iRow]["FULLOREMPTY"].Equals("F"))
                        {
                            if (dt.Rows[iRow]["SIZE_CON"].Equals("20"))
                            {
                                fullStatisticsE.Abnormal_F_20 = Convert.ToInt16(dt.Rows[iRow]["ABNORMALCON"]);
                            }
                            else if (dt.Rows[iRow]["SIZE_CON"].Equals("40"))
                            {
                                fullStatisticsE.Abnormal_F_40 = Convert.ToInt16(dt.Rows[iRow]["ABNORMALCON"]);
                            }
                            else
                            {
                                fullStatisticsE.Abnormal_F_other = Convert.ToInt16(dt.Rows[iRow]["ABNORMALCON"]);
                            }
                        }
                    }

                }


                int[] strArray = new int[27];
                strArray[0] = fullStatisticsE.Forecast_total;
                strArray[1] = fullStatisticsE.Forecast_E_20;
                strArray[2] = fullStatisticsE.Forecast_E_40;
                strArray[3] = fullStatisticsE.Forecast_E_other;
                strArray[4] = fullStatisticsE.Forecast_E_total;
                strArray[5] = fullStatisticsE.Forecast_F_20;
                strArray[6] = fullStatisticsE.Forecast_F_40;
                strArray[7] = fullStatisticsE.Forecast_F_other;
                strArray[8] = fullStatisticsE.Forecast_F_total;
                strArray[9] = fullStatisticsE.Tally_total;
                strArray[10] = fullStatisticsE.Tally_E_20;
                strArray[11] = fullStatisticsE.Tally_E_40;
                strArray[12] = fullStatisticsE.Tally_E_other;
                strArray[13] = fullStatisticsE.Tally_E_total;
                strArray[14] = fullStatisticsE.Tally_F_20;
                strArray[15] = fullStatisticsE.Tally_F_40;
                strArray[16] = fullStatisticsE.Tally_F_other;
                strArray[17] = fullStatisticsE.Tally_F_total;
                strArray[18] = fullStatisticsE.Abnormal_total;
                strArray[19] = fullStatisticsE.Abnormal_E_20;
                strArray[20] = fullStatisticsE.Abnormal_E_40;
                strArray[21] = fullStatisticsE.Abnormal_E_other;
                strArray[22] = fullStatisticsE.Abnormal_E_total;
                strArray[23] = fullStatisticsE.Abnormal_F_20;
                strArray[24] = fullStatisticsE.Abnormal_F_40;
                strArray[25] = fullStatisticsE.Abnormal_F_other;
                strArray[26] = fullStatisticsE.Abnormal_F_total;


                Json = JsonConvert.SerializeObject(new DicPackage(true, strArray, null).DicInfo());
            }
            catch (Exception ex)
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, string.Format("{0}：获取全统计数据发生异常。{1}", ex.Source, ex.Message)).DicInfo());
            }
        }
        protected string Json;
    }
}