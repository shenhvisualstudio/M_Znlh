//
//文件名：    UploadShipImages.aspx.cs
//功能描述：  上传已修改贝船图数据
//创建时间：  2017/1/19
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
using static M_Znlh.Ship.ShipImageE;
using Leo.Oracle;

namespace M_Znlh.Ship
{
    public partial class UploadShipImages : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //身份校验
            if (!InterfaceTool.IdentityVerify(Request))
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, "身份认证错误！").DicInfo());
                return;
            }

            //船图列表
            string strShipImageList = Request.Params["ShipImageList"];

            AppLog log = new AppLog(Request);
            log.strBehavior = "上传已修改贝船图数据";
            log.strBehaviorURL = "/Ship/UploadShipImages.aspx";
   
            try
            {
                if (strShipImageList == null)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(false, null, "参数错误，上传已修改贝船图数据失败！").DicInfo());
                    return;
                }

                List<ShipImage> list = JsonConvert.DeserializeObject<List<ShipImage>>(strShipImageList);

                log.strAccount = list[0].modifier;

                List<string> containerNoList = new List<string>();

                for (int i = 0; i < list.Count; i++)
                {

                    ShipImage shipImage = list[i];

                    string strTempContainerNo = shipImage.container_no;
                    string strTempBaynum = shipImage.baynum;
                    
                    //箱号不能为空
                    if (!string.IsNullOrWhiteSpace(strTempContainerNo) && !string.IsNullOrWhiteSpace(strTempBaynum) && !strTempBaynum.Equals("null") && containerNoList.IndexOf(strTempContainerNo) == -1)
                    {

                        //遍历是否有重复箱号的数据
                        int j;
                        for (j = i + 1; j < list.Count; j++)
                        {

                            if (strTempContainerNo.Equals(list[j].container_no)&& list[j].bayno != null && !list[j].bayno.Equals("null") && !list[j].bayno.Equals(""))
                            {
                                if (!string.IsNullOrWhiteSpace(shipImage.baynum)&& !shipImage.baynum.Equals("null")) {
                                    if (Convert.ToInt16(shipImage.baynum) > Convert.ToInt16(list[j].baynum))
                                    {

                                        shipImage = list[j];
                                    }
                                } 
                            }
                        }
                        if (i == list.Count - 1)
                        {

                            shipImage = list[i];

                        }

                        containerNoList.Add(shipImage.container_no);

                        string strOperareName = string.Empty;
                        string strSql = string.Format(@"select distinct name
                                                        from SYSTEM_USER_TABLE 
                                                        where user_id='{0}'", 
                                                        shipImage.modifier);
                        var dt = new Leo.SqlServer.DataAccess(RegistryKey.KeyPathTallySqlServer).ExecuteTable(strSql);
                        if (dt.Rows.Count > 0) {

                            strOperareName = dt.Rows[0]["name"].ToString();
                        }
         
                        strSql = string.Format(@"select MOVED,BAYNO,OLDBAYNO,USER_NAME,SIZE_CON,WORK_NO
                                                 from CON_IMAGE 
                                                 where SHIP_ID='{0}' and CONTAINER_NO='{1}'",
                                                 shipImage.ship_id, shipImage.container_no);
                        dt = da.ExecuteTable(strSql);
                        if (dt.Rows.Count <= 0)
                        {
                            Json = JsonConvert.SerializeObject(new DicPackage(false, null, "暂无数据！").DicInfo());
                            log.LogCatalogFailure(string.Format("上传已修改贝船图数据发生异常。航次'{0}'、箱号'{1}'不存在", shipImage.ship_id, shipImage.container_no));
                            return;
                        }

                        string strBayno = string.Empty;
                        string strOldbayno = string.Empty;
                        string strUserName = string.Empty;
                        if (shipImage.bayno != null && !shipImage.bayno.Equals("null"))
                        {
                            strBayno = shipImage.bayno;
                        }
                        if (dt.Rows[0]["BAYNO"].ToString() != null && !dt.Rows[0]["BAYNO"].ToString().Equals("null"))
                        {
                            strOldbayno = dt.Rows[0]["BAYNO"].ToString();
                        }
                        if (strOperareName != null && !strOperareName.Equals("null"))
                        {
                            strUserName = strOperareName;
                        }

                        //保留原船图数据记录
                        strSql = string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,OLDBAYNO,MOVED,USER_NAME,BAY_OPERNAME,BAY_OPERTIME,SIZE_CON,WORK_NO,MOVEDBAYNO) 
                                                 values('{0}','{1}','{2}','{3}','{4}','{5}','{6}',to_date('{7}','yyyy-mm-dd HH24:mi:ss'),'{8}','{9}','{10}')", 
                                                 shipImage.ship_id, shipImage.container_no, dt.Rows[0]["BAYNO"].ToString(), dt.Rows[0]["OLDBAYNO"].ToString(),
                                                 dt.Rows[0]["MOVED"].ToString(), dt.Rows[0]["USER_NAME"].ToString(), shipImage.modifier, shipImage.modifytime, 
                                                 dt.Rows[0]["SIZE_CON"].ToString(), dt.Rows[0]["WORK_NO"].ToString(), strBayno);
                        da.ExecuteNonQuery(strSql);

                        //更新
                        strSql = string.Format(@"update CON_IMAGE 
                                                 set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                                 where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                                 strBayno, strOldbayno, shipImage.modifier, shipImage.modifytime, strUserName, shipImage.ship_id, shipImage.container_no);

                        da.ExecuteNonQuery(strSql);
                    }
                }

                da.CommitTransaction();


                Json = JsonConvert.SerializeObject(new DicPackage(true, null, "上传成功！").DicInfo());
                log.LogCatalogSuccess();
            }
            catch (Exception ex)
            {
                da.RollbackTransaction();
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, string.Format("{0}：上传已修改贝船图数据发生异常。{1}", ex.Source, ex.Message)).DicInfo());
                log.LogCatalogFailure(string.Format("{0}：上传已修改贝船图数据发生异常。{1}", ex.Source, ex.Message));
            }
        }

        protected string Json;
        DataAccess da = (DataAccess)new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally);
    }
}



//strShipImageList = "[{\"bay_col\":\"02\",\"bay_num\":\"11\",\"bay_row\":\"04\",\"baycol\":\"\",\"bayno\":\"\",\"baynum\":\"\",\"bayrow\":\"\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"\",\"code_load_port\":\"\",\"code_unload_port\":\"\",\"container_no\":\"\",\"container_type\":\"\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"446\",\"image_id\":\"605957\",\"inoutmark\":\"0\",\"jbayno\":\"100204\",\"joint\":\"1\",\"location\":\"cabin\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"020923\",\"modifytime\":\"2017 / 03 / 07 02:57:42\",\"moved\":\"1\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"\",\"sbayno\":\"110204\",\"screen_col\":2,\"screen_row\":2,\"sealno\":\"\",\"ship_id\":\"1749\",\"size_con\":\"\",\"tbayno\":\"120204\",\"transmark\":\"N\",\"unload_mark\":\"\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"\",\"work_date\":\"2017 / 3 / 7 12:10:14\",\"work_no\":\"\"},{\"bay_col\":\"02\",\"bay_num\":\"11\",\"bay_row\":\"04\",\"baycol\":\"\",\"bayno\":\"\",\"baynum\":\"\",\"bayrow\":\"\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"\",\"code_load_port\":\"\",\"code_unload_port\":\"\",\"container_no\":\"\",\"container_type\":\"\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"446\",\"image_id\":\"605957\",\"inoutmark\":\"0\",\"jbayno\":\"100204\",\"joint\":\"1\",\"location\":\"cabin\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"020923\",\"modifytime\":\"2017 / 03 / 07 02:57:42\",\"moved\":\"1\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"\",\"sbayno\":\"110204\",\"screen_col\":2,\"screen_row\":2,\"sealno\":\"\",\"ship_id\":\"1749\",\"size_con\":\"\",\"tbayno\":\"120204\",\"transmark\":\"N\",\"unload_mark\":\"\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"\",\"work_date\":\"2017 / 3 / 7 12:10:14\",\"work_no\":\"\"}]";

//strShipImageList = "[{\"bay_col\":\"05\",\"bay_num\":\"01\",\"bay_row\":\"04\",\"baycol\":\"\",\"bayno\":\"010582\",\"baynum\":\"\",\"bayrow\":\"\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"\",\"code_load_port\":\"\",\"code_unload_port\":\"\",\"container_no\":\"\",\"container_type\":\"\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"446\",\"image_id\":\"605957\",\"inoutmark\":\"0\",\"jbayno\":\"100204\",\"joint\":\"1\",\"location\":\"cabin\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"020923\",\"modifytime\":\"2017/03/07 02:57:42\",\"moved\":\"0\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"\",\"sbayno\":\"110204\",\"screen_col\":2,\"screen_row\":2,\"sealno\":\"\",\"ship_id\":\"195\",\"size_con\":\"\",\"tbayno\":\"120204\",\"transmark\":\"N\",\"unload_mark\":\"0\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"\",\"work_date\":\"2017/3/7 12:10:14\",\"work_no\":\"\"},{\"bay_col\":\"05\",\"bay_num\":\"01\",\"bay_row\":\"04\",\"baycol\":\"\",\"bayno\":\"010584\",\"baynum\":\"\",\"bayrow\":\"\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"\",\"code_load_port\":\"\",\"code_unload_port\":\"\",\"container_no\":\"GVCU2155415\",\"container_type\":\"\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"446\",\"image_id\":\"605957\",\"inoutmark\":\"0\",\"jbayno\":\"100204\",\"joint\":\"1\",\"location\":\"cabin\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"020923\",\"modifytime\":\"2017/03/07 02:57:42\",\"moved\":\"0\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"\",\"sbayno\":\"110204\",\"screen_col\":2,\"screen_row\":2,\"sealno\":\"\",\"ship_id\":\"195\",\"size_con\":\"\",\"tbayno\":\"120204\",\"transmark\":\"N\",\"unload_mark\":\"0\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"\",\"work_date\":\"2017/3/7 12:10:14\",\"work_no\":\"\"}]";

//strShipImageList =   "[{\"bay_col\":\"02\",\"bay_num\":\"01\",\"bay_row\":\"82\",\"baycol\":\"02\",\"bayno\":\"010282\",\"baynum\":\"01\",\"bayrow\":\"82\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"F\",\"code_load_port\":\"CNLYG\",\"code_unload_port\":\"CNSHA\",\"container_no\":\"ZGXU2116914\",\"container_type\":\"GP\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"9470\",\"image_id\":\"\",\"inoutmark\":\"0\",\"jbayno\":\"\",\"joint\":\"1\",\"location\":\"board\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"294\",\"modifytime\":\"2017/05/11 02:12:39\",\"moved\":\"0\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"\",\"sbayno\":\"010282\",\"screen_col\":3,\"screen_row\":2,\"sealno\":\"\",\"ship_id\":\"196\",\"size_con\":\"20\",\"tbayno\":\"020282\",\"transmark\":\"N\",\"unload_mark\":\"0\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"26400\",\"work_date\":\"\",\"work_no\":\"\"},{\"bay_col\":\"04\",\"bay_num\":\"01\",\"bay_row\":\"06\",\"baycol\":\"\",\"bayno\":\"\",\"baynum\":\"\",\"bayrow\":\"\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"\",\"code_load_port\":\"\",\"code_unload_port\":\"\",\"container_no\":\"\",\"container_type\":\"\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"9900\",\"image_id\":\"812363\",\"inoutmark\":\"0\",\"jbayno\":\"\",\"joint\":\"1\",\"location\":\"cabin\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"wlgc\",\"modifytime\":\"2017/05/11 02:12:39\",\"moved\":\"1\",\"moved_name\":\"捣箱\",\"night\":\"null\",\"oldbayno\":\"010406\",\"sbayno\":\"010406\",\"screen_col\":1,\"screen_row\":3,\"sealno\":\"\",\"ship_id\":\"196\",\"size_con\":\"\",\"tbayno\":\"020406\",\"transmark\":\"N\",\"unload_mark\":\"\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"\",\"work_date\":\"\",\"work_no\":\"\"}]";

//strShipImageList = "[{\"bay_col\":\"04\",\"bay_num\":\"03\",\"bay_row\":\"02\",\"baycol\":\"04\",\"bayno\":\"030402\",\"baynum\":\"03\",\"bayrow\":\"02\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"F\",\"code_load_port\":\"CNLYG\",\"code_unload_port\":\"CNSHA\",\"container_no\":\"MSKU3478257\",\"container_type\":\"GP\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"10248\",\"image_id\":\"812223\",\"inoutmark\":\"0\",\"jbayno\":\"020402\",\"joint\":\"1\",\"location\":\"cabin\",\"mark_/*modify*/\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"wlgc\",\"modifytime\":\"2017/05/15 08:09:38\",\"moved\":\"0\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"030402\",\"sbayno\":\"030402\",\"screen_col\":1,\"screen_row\":1,\"sealno\":\"\",\"ship_id\":\"196\",\"size_con\":\"20\",\"tbayno\":\"040402\",\"transmark\":\"N\",\"unload_mark\":\"1\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"27000\",\"work_date\":\"2017/4/27 10:37:36\",\"work_no\":\"021044\"},{\"bay_col\":\"02\",\"bay_num\":\"03\",\"bay_row\":\"02\",\"baycol\":\"02\",\"bayno\":\"030202\",\"baynum\":\"03\",\"bayrow\":\"02\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"F\",\"code_load_port\":\"CNLYG\",\"code_unload_port\":\"CNSHA\",\"container_no\":\"MRKU7349678\",\"container_type\":\"GP\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"10249\",\"image_id\":\"812252\",\"inoutmark\":\"0\",\"jbayno\":\"020202\",\"joint\":\"1\",\"location\":\"cabin\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"294\",\"modifytime\":\"2017/05/15 08:09:38\",\"moved\":\"0\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"030202\",\"sbayno\":\"030202\",\"screen_col\":2,\"screen_row\":1,\"sealno\":\"\",\"ship_id\":\"196\",\"size_con\":\"20\",\"tbayno\":\"040202\",\"transmark\":\"N\",\"unload_mark\":\"1\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"28000\",\"work_date\":\"2017/4/27 10:38:24\",\"work_no\":\"021044\"}]";

//strShipImageList = "[{\"bay_col\":\"06\",\"bay_num\":\"01\",\"bay_row\":\"84\",\"baycol\":\"06\",\"bayno\":\"010684\",\"baynum\":\"02\",\"bayrow\":\"82\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"F\",\"code_load_port\":\"CNLYG\",\"code_unload_port\":\"CNSHA\",\"container_no\":\"ECMU9156563\",\"container_type\":\"HC\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"216\",\"image_id\":\"812400\",\"inoutmark\":\"0\",\"jbayno\":\"\",\"joint\":\"1\",\"location\":\"board\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"294\",\"modifytime\":\"2017 / 05 / 16 03:05:43\",\"moved\":\"0\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"020684\",\"sbayno\":\"010684\",\"screen_col\":1,\"screen_row\":3,\"sealno\":\"\",\"ship_id\":\"196\",\"size_con\":\"40\",\"tbayno\":\"020684\",\"transmark\":\"N\",\"unload_mark\":\"0\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"26800\",\"work_date\":\"\",\"work_no\":\"\"},{\"bay_col\":\"06\",\"bay_num\":\"03\",\"bay_row\":\"84\",\"baycol\":\"02\",\"bayno\":\"\",\"baynum\":\"\",\"bayrow\":\"82\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"F\",\"code_load_port\":\"CNLYG\",\"code_unload_port\":\"CNSHA\",\"container_no\":\"ECMU9156563\",\"container_type\":\"HC\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"217\",\"image_id\":\"812400\",\"inoutmark\":\"0\",\"jbayno\":\"020684\",\"joint\":\"1\",\"location\":\"board\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"294\",\"modifytime\":\"2017 / 05 / 16 03:05:43\",\"moved\":\"0\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"020684\",\"sbayno\":\"030684\",\"screen_col\":1,\"screen_row\":3,\"sealno\":\"\",\"ship_id\":\"196\",\"size_con\":\"40\",\"tbayno\":\"040684\",\"transmark\":\"N\",\"unload_mark\":\"0\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"26800\",\"work_date\":\"\",\"work_no\":\"\"},{\"bay_col\":\"02\",\"bay_num\":\"01\",\"bay_row\":\"82\",\"baycol\":\"06\",\"bayno\":\"020282\",\"baynum\":\"02\",\"bayrow\":\"84\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"F\",\"code_load_port\":\"CNLYG\",\"code_unload_port\":\"CNSHA\",\"container_no\":\"ZGXU6125940\",\"container_type\":\"HC\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"381\",\"image_id\":\"812183\",\"inoutmark\":\"0\",\"jbayno\":\"\",\"joint\":\"1\",\"location\":\"board\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"294\",\"modifytime\":\"2017 / 05 / 16 03:05:43\",\"moved\":\"0\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"\",\"sbayno\":\"010282\",\"screen_col\":3,\"screen_row\":2,\"sealno\":\"\",\"ship_id\":\"196\",\"size_con\":\"40\",\"tbayno\":\"020282\",\"transmark\":\"N\",\"unload_mark\":\"0\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"28800\",\"work_date\":\"\",\"work_no\":\"\"},{\"bay_col\":\"02\",\"bay_num\":\"03\",\"bay_row\":\"82\",\"baycol\":\"06\",\"bayno\":\"020282\",\"baynum\":\"02\",\"bayrow\":\"84\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"F\",\"code_load_port\":\"CNLYG\",\"code_unload_port\":\"CNSHA\",\"container_no\":\"ZGXU6125940\",\"container_type\":\"HC\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"382\",\"image_id\":\"812183\",\"inoutmark\":\"0\",\"jbayno\":\"020282\",\"joint\":\"1\",\"location\":\"board\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"294\",\"modifytime\":\"2017 / 05 / 16 03:05:43\",\"moved\":\"0\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"\",\"sbayno\":\"030282\",\"screen_col\":3,\"screen_row\":2,\"sealno\":\"\",\"ship_id\":\"196\",\"size_con\":\"40\",\"tbayno\":\"040282\",\"transmark\":\"N\",\"unload_mark\":\"0\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"28800\",\"work_date\":\"\",\"work_no\":\"\"}]";

//strShipImageList = "[{\"bay_col\":\"06\",\"bay_num\":\"23\",\"bay_row\":\"84\",\"baycol\":\"04\",\"bayno\":\"\",\"baynum\":\"\",\"bayrow\":\"06\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"E\",\"code_load_port\":\"CNTAO\",\"code_unload_port\":\"CNLYG\",\"container_no\":\"HALU2002934\",\"container_type\":\"GP\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"2553\",\"image_id\":\"722771\",\"inoutmark\":\"1\",\"jbayno\":\"220684\",\"joint\":\"0\",\"location\":\"board\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"294\",\"modifytime\":\"2017 / 05 / 17 07:59:44\",\"moved\":\"0\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"220684\",\"sbayno\":\"230684\",\"screen_col\":1,\"screen_row\":3,\"sealno\":\"\",\"ship_id\":\"195\",\"size_con\":\"20\",\"tbayno\":\"\",\"transmark\":\"N\",\"unload_mark\":\"0\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"2300\",\"work_date\":\"2017 / 4 / 10 15:36:22\",\"work_no\":\"\"},{\"bay_col\":\"04\",\"bay_num\":\"23\",\"bay_row\":\"06\",\"baycol\":\"06\",\"bayno\":\"220406\",\"baynum\":\"22\",\"bayrow\":\"84\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"E\",\"code_load_port\":\"CNTAO\",\"code_unload_port\":\"CNLYG\",\"container_no\":\"TTNU9329926\",\"container_type\":\"HC\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"1\",\"id\":\"2567\",\"image_id\":\"\",\"inoutmark\":\"1\",\"jbayno\":\"220406\",\"joint\":\"0\",\"location\":\"cabin\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"294\",\"modifytime\":\"2017 / 05 / 17 07:59:44\",\"moved\":\"0\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"210406\",\"sbayno\":\"230406\",\"screen_col\":1,\"screen_row\":3,\"sealno\":\"\",\"ship_id\":\"195\",\"size_con\":\"40\",\"tbayno\":\"\",\"transmark\":\"N\",\"unload_mark\":\"1\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"4000\",\"work_date\":\"\",\"work_no\":\"021044\"},{\"bay_col\":\"04\",\"bay_num\":\"21\",\"bay_row\":\"06\",\"baycol\":\"06\",\"bayno\":\"220406\",\"baynum\":\"22\",\"bayrow\":\"84\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"E\",\"code_load_port\":\"CNTAO\",\"code_unload_port\":\"CNLYG\",\"container_no\":\"TTNU9329926\",\"container_type\":\"HC\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"1\",\"id\":\"2593\",\"image_id\":\"814293\",\"inoutmark\":\"1\",\"jbayno\":\"200406\",\"joint\":\"1\",\"location\":\"cabin\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"294\",\"modifytime\":\"2017 / 05 / 17 07:59:44\",\"moved\":\"0\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"210406\",\"sbayno\":\"210406\",\"screen_col\":1,\"screen_row\":3,\"sealno\":\"\",\"ship_id\":\"195\",\"size_con\":\"40\",\"tbayno\":\"220406\",\"transmark\":\"N\",\"unload_mark\":\"1\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"4000\",\"work_date\":\"\",\"work_no\":\"021044\"},{\"bay_col\":\"06\",\"bay_num\":\"21\",\"bay_row\":\"84\",\"baycol\":\"06\",\"bayno\":\"210684\",\"baynum\":\"22\",\"bayrow\":\"06\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"E\",\"code_load_port\":\"CNTAO\",\"code_unload_port\":\"CNLYG\",\"container_no\":\"HALU2002934\",\"container_type\":\"GP\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"2598\",\"image_id\":\"722771\",\"inoutmark\":\"1\",\"jbayno\":\"200684\",\"joint\":\"1\",\"location\":\"board\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"294\",\"modifytime\":\"2017 / 05 / 17 07:59:44\",\"moved\":\"0\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"220684\",\"sbayno\":\"210684\",\"screen_col\":1,\"screen_row\":3,\"sealno\":\"\",\"ship_id\":\"195\",\"size_con\":\"20\",\"tbayno\":\"220684\",\"transmark\":\"N\",\"unload_mark\":\"0\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"2300\",\"work_date\":\"2017 / 4 / 10 15:36:22\",\"work_no\":\"\"}]";

//strShipImageList = " [{\"bay_col\":\"01\",\"bay_num\":\"03\",\"bay_row\":\"06\",\"baycol\":\"01\",\"bayno\":\"020106\",\"baynum\":\"02\",\"bayrow\":\"82\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"P17\",\"code_empty\":\"F\",\"code_load_port\":\"CNTAO\",\"code_unload_port\":\"CNSHA\",\"container_no\":\"RFCU4024520\",\"container_type\":\"HC\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"2088\",\"image_id\":\"\",\"inoutmark\":\"0\",\"jbayno\":\"020106\",\"joint\":\"1\",\"location\":\"cabin\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"215\",\"modifytime\":\"2017/06/10 08:06:58\",\"moved\":\"1\",\"moved_name\":\"捣箱\",\"night\":\"null\",\"oldbayno\":\"010106\",\"sbayno\":\"030106\",\"screen_col\":4,\"screen_row\":3,\"sealno\":\"\",\"ship_id\":\"195\",\"size_con\":\"40\",\"tbayno\":\"040106\",\"transmark\":\"N\",\"unload_mark\":\"0\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"30000\",\"work_date\":\"\",\"work_no\":\"\"},{\"bay_col\":\"01\",\"bay_num\":\"01\",\"bay_row\":\"06\",\"baycol\":\"01\",\"bayno\":\"020106\",\"baynum\":\"02\",\"bayrow\":\"82\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"P17\",\"code_empty\":\"F\",\"code_load_port\":\"CNTAO\",\"code_unload_port\":\"CNSHA\",\"container_no\":\"RFCU4024520\",\"container_type\":\"HC\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"2119\",\"image_id\":\"814367\",\"inoutmark\":\"0\",\"jbayno\":\"\",\"joint\":\"1\",\"location\":\"cabin\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"215\",\"modifytime\":\"2017/06/10 08:06:58\",\"moved\":\"1\",\"moved_name\":\"捣箱\",\"night\":\"null\",\"oldbayno\":\"010106\",\"sbayno\":\"010106\",\"screen_col\":4,\"screen_row\":3,\"sealno\":\"\",\"ship_id\":\"195\",\"size_con\":\"40\",\"tbayno\":\"020106\",\"transmark\":\"N\",\"unload_mark\":\"0\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"30000\",\"work_date\":\"\",\"work_no\":\"\"},{\"bay_col\":\"01\",\"bay_num\":\"01\",\"bay_row\":\"82\",\"baycol\":\"01\",\"bayno\":\"010182\",\"baynum\":\"01\",\"bayrow\":\"06\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"E\",\"code_load_port\":\"CNTAO\",\"code_unload_port\":\"CNLYG\",\"container_no\":\"TEMU5960479\",\"container_type\":\"GP\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"2125\",\"image_id\":\"814336\",\"inoutmark\":\"1\",\"jbayno\":\"\",\"joint\":\"1\",\"location\":\"board\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"215\",\"modifytime\":\"2017/06/10 08:06:58\",\"moved\":\"0\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"\",\"sbayno\":\"010182\",\"screen_col\":4,\"screen_row\":2,\"sealno\":\"\",\"ship_id\":\"195\",\"size_con\":\"20\",\"tbayno\":\"020182\",\"transmark\":\"N\",\"unload_mark\":\"0\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"2300\",\"work_date\":\"\",\"work_no\":\"\"},{\"bay_col\":\"01\",\"bay_num\":\"03\",\"bay_row\":\"82\",\"baycol\":\"01\",\"bayno\":\"\",\"baynum\":\"\",\"bayrow\":\"06\",\"chi_vessel\":\"新海悦\",\"code_crane\":\"\",\"code_empty\":\"E\",\"code_load_port\":\"CNTAO\",\"code_unload_port\":\"CNLYG\",\"container_no\":\"TEMU5960479\",\"container_type\":\"GP\",\"danger_grade\":\"\",\"degree_setting\":\"\",\"degree_unit\":\"\",\"delivery\":\"\",\"eng_vessel\":\"XIN HAI YUE\",\"holidays\":\"\",\"id\":\"2134\",\"image_id\":\"814336\",\"inoutmark\":\"1\",\"jbayno\":\"020182\",\"joint\":\"1\",\"location\":\"board\",\"mark_modify\":\"1\",\"max_degree\":\"\",\"min_degree\":\"\",\"modifier\":\"215\",\"modifytime\":\"2017/06/10 08:06:58\",\"moved\":\"0\",\"moved_name\":\"\",\"night\":\"null\",\"oldbayno\":\"\",\"sbayno\":\"030182\",\"screen_col\":4,\"screen_row\":2,\"sealno\":\"\",\"ship_id\":\"195\",\"size_con\":\"20\",\"tbayno\":\"040182\",\"transmark\":\"N\",\"unload_mark\":\"0\",\"user_char\":\"1\",\"v_id\":\"6321\",\"weight\":\"2300\",\"work_date\":\"\",\"work_no\":\"\"}]";



//if (strMoved.Equals(shipImage.moved))
//{
//    ////更新
//    //strSql = string.Format(@"update CON_IMAGE 
//    //                     set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
//    //                     where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
//    //                     shipImage.bayno, shipImage.oldbayno, shipImage.modifier, shipImage.modifytime, strOperareName, shipImage.ship_id, shipImage.container_no);


//    //更新
//    strSql = string.Format(@"update CON_IMAGE 
//                         set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
//                         where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
//                         shipImage.bayno, dt.Rows[0]["BAYNO"].ToString(), shipImage.modifier, shipImage.modifytime, strOperareName, shipImage.ship_id, shipImage.container_no);
//}
//else
//{
//    ////更新
//    //strSql = string.Format(@"update CON_IMAGE 
//    //                     set BAYNO='{0}',OLDBAYNO='{1}',MOVED='{2}',BAY_OPERNAME='{3}',BAY_OPERTIME=to_date('{4}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{5}'
//    //                     where SHIP_ID='{6}' and CONTAINER_NO='{7}'",
//    //                     shipImage.bayno, shipImage.oldbayno, shipImage.moved, shipImage.modifier, shipImage.modifytime, strOperareName, shipImage.ship_id, shipImage.container_no);

//    //更新
//    strSql = string.Format(@"update CON_IMAGE 
//                         set BAYNO='{0}',OLDBAYNO='{1}',MOVED='{2}',BAY_OPERNAME='{3}',BAY_OPERTIME=to_date('{4}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{5}'
//                         where SHIP_ID='{6}' and CONTAINER_NO='{7}'",
//                         shipImage.bayno, dt.Rows[0]["BAYNO"].ToString(), shipImage.moved, shipImage.modifier, shipImage.modifytime, strOperareName, shipImage.ship_id, shipImage.container_no);
//}