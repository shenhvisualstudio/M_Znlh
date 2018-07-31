using Leo.Oracle;
using Newtonsoft.Json;
using ServiceInterface.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace M_Znlh.Move
{

    public class IMoveBay
    {
        /// <summary>
        /// 数据库连接对象
        /// </summary>
        private DataAccess da = null;

        /// <summary>
        /// 日志对象
        /// </summary>
        private AppLog log;

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="log"></param>
        public IMoveBay(AppLog log) {
            this.log = log;
        }

        #endregion

        #region 设置数据库连接对象
        /// <summary>
        /// 设置数据库连接对象
        /// </summary>
        /// <param name="da">数据库连接对象</param>
        public void setDataAccess(DataAccess da) {
            this.da = da;
        }

        #endregion

        #region 校验目标贝位号是否存在
        /// <summary>
        /// 校验目标贝位号是否存在
        /// </summary>
        /// <param name="shipId">船舶ID</param>
        /// <param name="bayno">贝位号</param>
        /// <returns></returns>
        public bool isExistBayno(string v_id, string bayno)
        {
            if (da == null) {
                return false;
            }
            string strSql = string.Format(@"select count(bayno) as count
                                           from vbay_no t
                                           where v_id={0} and user_char=1 and (bayno='{1}' or tbayno='{1}')",
                                           v_id, bayno);
            var dt = da.ExecuteTable(strSql);
            if (Convert.ToInt16(dt.Rows[0]["count"]) == 0)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region 移贝
        /// <summary>
        /// 移贝20
        /// </summary>
        /// <param name="strShipId">航次ID</param>
        /// <param name="strContainerNo">待调集装箱号</param>
        /// <param name="strBayno">待调集装箱贝位号</param>
        /// <param name="strEndBayno">目标贝位号（奇数）</param>
        /// <param name="strOperareName">操作人</param>
        /// <returns></returns>
        public string MoveBayOfTwenty(string strShipId, string strContainerNo, string strBayno, string strEndBayno, string strOperareName)
        {
            //目标位置前通
            String frontEndBayno_40 = Convert.ToString((Convert.ToInt16(strEndBayno.Substring(0, 2)) - 1)).PadLeft(2, '0') + strEndBayno.Substring(2).PadLeft(4, '0');
            //目标位置后通
            String backEndBayno_40 = Convert.ToString((Convert.ToInt16(strEndBayno.Substring(0, 2)) + 1)).PadLeft(2, '0') + strEndBayno.Substring(2).PadLeft(4, '0');

            string strSql =
                string.Format(@"select t.size_con,t.bayno,t.container_no 
                                from con_image t 
                                where ship_id={0} and (bayno = '{1}' or bayno = '{2}')",
                                strShipId, frontEndBayno_40, backEndBayno_40);
            var endDt = da.ExecuteTable(strSql);
            if (endDt.Rows.Count <= 0)
            {

                strSql =
                    string.Format(@"select t.size_con,t.bayno,t.container_no,t.unload_mark 
                                    from con_image t 
                                    where ship_id={0} and bayno = '{1}'",
                                    strShipId, strEndBayno);
                endDt = da.ExecuteTable(strSql);
                if (endDt.Rows.Count <= 0)
                {
                    //20移至空位置
                    MoveBayOfContainerToNull(strShipId, strContainerNo, strBayno, strEndBayno, strOperareName);
                }
                else if (endDt.Rows.Count == 1)
                {
                    string strEndContainerNo = Convert.ToString(endDt.Rows[0]["container_no"]);
                    string strEndUnloadMark = Convert.ToString(endDt.Rows[0]["unload_mark"]);
                    int strSizeCon = Convert.ToInt16(endDt.Rows[0]["size_con"]);
                    if (strSizeCon == 40)
                    {
                        log.LogCatalogFailure(string.Format("船图数据有误,集装箱尺寸与实际贝位不匹配"));
                        return JsonConvert.SerializeObject(new DicPackage(false, null, "船图数据有误,集装箱尺寸与实际贝位不匹配").DicInfo());
                    }

                    if (strEndUnloadMark.Equals("1"))
                    {
                        log.LogCatalogFailure(string.Format("目标贝位号" + strEndBayno + "(" + strEndContainerNo + ")" + "已作业"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strEndBayno + "(" + strEndContainerNo + ")" + "已作业").DicInfo());
                    }


                    //待调位置前通
                    String strFrontBayno_40 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) - 1)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');
                    //待调位置后通
                    String strBackBayno_40 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) + 1)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');


                    //判断待调位置是否有被确认箱子
                    strSql =
                        string.Format(@"select bayno,container_no
                                        from con_image 
                                        where ship_id={0} and (bayno='{1}' or bayno='{2}' or bayno='{3}') and container_no <> '{4}' and unload_mark='1'",
                                        strShipId, strBayno, strFrontBayno_40, strBackBayno_40, strContainerNo);
                    var dt = da.ExecuteTable(strSql);
                    if (dt.Rows.Count <= 0)
                    {
                        //20移至20
                        MoveBayOfContainerToContainer(strShipId, strContainerNo, strBayno, strEndContainerNo, strEndBayno, strOperareName);
                    }
                    else
                    {
                        string strBaynos = string.Empty;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            strBaynos += Convert.ToString(dt.Rows[i]["bayno"]) + "(" + Convert.ToString(dt.Rows[i]["container_no"]) + ")";
                            strBaynos += ",";
                        }
                        strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                        log.LogCatalogFailure(string.Format("待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置").DicInfo());
                    }

                }
                else if (endDt.Rows.Count > 1)
                {
                    string strBaynos = string.Empty;
                    for (int i = 0; i < endDt.Rows.Count; i++)
                    {
                        strBaynos += Convert.ToString(endDt.Rows[i]["bayno"]) + "(" + Convert.ToString(endDt.Rows[i]["container_no"]) + ")";
                        strBaynos += ",";
                    }
                    strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                    log.LogCatalogFailure(string.Format("目标贝位号" + strBaynos + "存在多个小箱，如需继续调贝，先将此贝位箱子移至空位置"));
                    return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strBaynos + "存在多个小箱，如需继续调贝，先将此贝位箱子移至空位置").DicInfo());
                }
            }
            else
            {
                //前通或后通存在箱子，此时无法调贝
                string strBaynos = string.Empty;
                for (int i = 0; i < endDt.Rows.Count; i++) {
                    strBaynos += Convert.ToString(endDt.Rows[i]["bayno"]) + "(" + Convert.ToString(endDt.Rows[i]["container_no"]) + ")";
                    strBaynos += ",";
                }
                strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                log.LogCatalogFailure(string.Format("目标贝位号" + strBaynos + "存在大箱子，如需继续调贝，先将此贝位箱子移至空位置"));
                return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strBaynos + "存在大箱子，如需继续调贝，先将此贝位箱子移至空位置").DicInfo());
            }
            return null;
        }

        /// <summary>
        /// 移贝40
        /// </summary>
        /// <param name="strShipId">航次ID</param>
        /// <param name="strContainerNo">待调集装箱号</param>
        /// <param name="strBayno">待调集装箱贝位号</param>
        /// <param name="strEndBayno">目标贝位号（偶数）</param>
        /// <param name="strOperareName">操作人</param>
        /// <returns></returns>
        public string MoveBayOfForty(string strShipId, string strContainerNo, string strBayno, string strEndBayno, string strOperareName)
        {
            //目标位置前通40
            String strFrontEndBayno_40 = Convert.ToString((Convert.ToInt16(strEndBayno.Substring(0, 2)) - 2)).PadLeft(2, '0') + strEndBayno.Substring(2).PadLeft(4, '0');
            //目标位置后通40
            String strBackEndBayno_40 = Convert.ToString((Convert.ToInt16(strEndBayno.Substring(0, 2)) + 2)).PadLeft(2, '0') + strEndBayno.Substring(2).PadLeft(4, '0');
            //目标位置前20
            String strFrontEndBayno_20 = Convert.ToString((Convert.ToInt16(strEndBayno.Substring(0, 2)) - 1)).PadLeft(2, '0') + strEndBayno.Substring(2).PadLeft(4, '0');
            //目标位置后20
            String strBackEndBayno_20 = Convert.ToString((Convert.ToInt16(strEndBayno.Substring(0, 2)) + 1)).PadLeft(2, '0') + strEndBayno.Substring(2).PadLeft(4, '0');


            string strSql =
                string.Format(@"select t.size_con,t.bayno,t.container_no 
                                from con_image t 
                                where ship_id={0} and (bayno = '{1}' or bayno = '{2}')",
                                strShipId, strFrontEndBayno_40, strBackEndBayno_40);
            var endDt = da.ExecuteTable(strSql);
            if (endDt.Rows.Count <= 0)
            {
                strSql =
                    string.Format(@"select t.size_con,t.bayno,t.container_no,t.unload_mark 
                                    from con_image t 
                                    where ship_id={0} and bayno = '{1}'",
                                    strShipId, strEndBayno);
                endDt = da.ExecuteTable(strSql);

                strSql =
                     string.Format(@"select t.size_con,t.bayno,t.container_no,t.unload_mark 
                                    from con_image t 
                                    where ship_id={0} and bayno = '{1}'",
                                    strShipId, strFrontEndBayno_20);
                var frontEndDt_20 = da.ExecuteTable(strSql);

                strSql =
                     string.Format(@"select t.size_con,t.bayno,t.container_no,t.unload_mark 
                                    from con_image t 
                                    where ship_id={0} and bayno = '{1}'",
                                    strShipId, strBackEndBayno_20);
                var backEndDt_20 = da.ExecuteTable(strSql);

                /************校验是否存在多箱**************/
                int endDtCount = endDt.Rows.Count;
                int frontEndDtCount_20 = frontEndDt_20.Rows.Count;
                int backEndDtCount_20 = backEndDt_20.Rows.Count;

                if (endDtCount > 1) {

                    //目标贝位存在多箱，此时无法调贝
                    string strBaynos = string.Empty;
                    for (int i = 0; i < endDtCount; i++)
                    {
                        strBaynos += Convert.ToString(endDt.Rows[i]["bayno"]) + "(" + Convert.ToString(endDt.Rows[i]["container_no"]) + ")";
                        strBaynos += ",";
                    }
                    strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                    log.LogCatalogFailure(string.Format("目标贝位号" + strBaynos + "存在多个大箱子，如需继续调贝，先将此贝位箱子移至空位置"));
                    return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strBaynos + "存在多个大箱子，如需继续调贝，先将此贝位箱子移至空位置").DicInfo());
                }

                if (frontEndDtCount_20 > 1)
                {

                    //目标贝位前20位置存在多箱，此时无法调贝
                    string strBaynos = string.Empty;
                    for (int i = 0; i < frontEndDtCount_20; i++)
                    {
                        strBaynos += Convert.ToString(frontEndDt_20.Rows[i]["bayno"]) + "(" + Convert.ToString(frontEndDt_20.Rows[i]["container_no"]) + ")";
                        strBaynos += ",";
                    }
                    strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                    log.LogCatalogFailure(string.Format("目标贝位号" + strBaynos + "存在多个小箱子，如需继续调贝，先将此贝位箱子移至空位置"));
                    return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strBaynos + "存在多个小箱子，如需继续调贝，先将此贝位箱子移至空位置").DicInfo());
                }


                if (backEndDtCount_20 > 1)
                {

                    //目标贝位后20位置存在多箱，此时无法调贝
                    string strBaynos = string.Empty;
                    for (int i = 0; i < backEndDtCount_20; i++)
                    {
                        strBaynos += Convert.ToString(backEndDt_20.Rows[i]["bayno"]) + "(" + Convert.ToString(backEndDt_20.Rows[i]["container_no"]) + ")";
                        strBaynos += ",";
                    }
                    strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                    log.LogCatalogFailure(string.Format("目标贝位号" + strBaynos + "存在多个小箱子，如需继续调贝，先将此贝位箱子移至空位置"));
                    return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strBaynos + "存在多个小箱子，如需继续调贝，先将此贝位箱子移至空位置").DicInfo());
                }

                if ((endDtCount == 1 && frontEndDtCount_20 == 1 && backEndDtCount_20 == 1) || (endDtCount == 1 && frontEndDtCount_20 == 1) || (endDtCount == 1 && backEndDtCount_20 == 1))
                {


                    //目标贝位、目标贝位前20位置、目标贝位后20位置同时存在一个箱子，此时无法调贝
                    string strBaynos = string.Empty;
                    for (int i = 0; i < endDtCount; i++)
                    {
                        strBaynos += Convert.ToString(endDt.Rows[i]["bayno"]) + "(" + Convert.ToString(endDt.Rows[i]["container_no"]) + ")";
                        strBaynos += ",";
                    }

                    for (int i = 0; i < frontEndDtCount_20; i++)
                    {
                        strBaynos += Convert.ToString(frontEndDt_20.Rows[i]["bayno"]) + "(" + Convert.ToString(frontEndDt_20.Rows[i]["container_no"]) + ")";
                        strBaynos += ",";
                    }

                    for (int i = 0; i < backEndDtCount_20; i++)
                    {
                        strBaynos += Convert.ToString(backEndDt_20.Rows[i]["bayno"]) + "(" + Convert.ToString(backEndDt_20.Rows[i]["container_no"]) + ")";
                        strBaynos += ",";
                    }

                    strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                    log.LogCatalogFailure(string.Format("目标贝位号" + strBaynos + "同时存在箱子，如需继续调贝，先将此贝位箱子移至空位置"));
                    return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strBaynos + "同时存在箱子，如需继续调贝，先将此贝位箱子移至空位置").DicInfo());
                }



                /************调贝**************/          
                if (endDtCount == 0 && frontEndDtCount_20 == 0 && backEndDtCount_20 == 0)
                {
                    //40移至空
                    MoveBayOfContainerToNull(strShipId, strContainerNo, strBayno, strEndBayno, strOperareName);
                }
                else if (endDtCount == 1 && frontEndDtCount_20 == 0 && backEndDtCount_20 == 0)
                {
                    string strEndContainerNo = Convert.ToString(endDt.Rows[0]["container_no"]);
                    string strEndUnloadMark = Convert.ToString(endDt.Rows[0]["unload_mark"]);
                    int strEndSizeCon = Convert.ToInt16(endDt.Rows[0]["size_con"]);
                    if (strEndSizeCon == 20)
                    {
                        log.LogCatalogFailure(string.Format("船图数据有误,集装箱尺寸与实际贝位不匹配"));
                        return JsonConvert.SerializeObject(new DicPackage(false, null, "船图数据有误,集装箱尺寸与实际贝位不匹配").DicInfo());
                    }

                    if (strEndUnloadMark.Equals("1"))
                    {
                        log.LogCatalogFailure(string.Format("目标贝位号" + strEndBayno + "(" + strEndContainerNo + ")" + "已作业"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strEndBayno + "(" + strEndContainerNo + ")" + "已作业").DicInfo());
                    }

                    //待调位置前通40
                    String strFrontBayno_40 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) - 2)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');
                    //待调位置后通40
                    String strBackBayno_40 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) + 2)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');
                    //待调位置前通20
                    String strFrontBayno_20 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) - 1)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');
                    //待调位置后通20
                    String strBackBayno_20 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) + 1)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');


                    //判断待调位置是否有被确认箱子
                    strSql =
                        string.Format(@"select bayno,container_no
                                        from con_image 
                                        where ship_id={0} and (bayno='{1}' or bayno='{2}' or bayno='{3}' or bayno='{4}' or bayno='{5}') and container_no <> '{6}' and unload_mark='1'",
                                        strShipId, strBayno, strFrontBayno_40, strBackBayno_40, strFrontBayno_20, strBackBayno_20, strContainerNo);
                    var dt = da.ExecuteTable(strSql);
                    if (dt.Rows.Count <= 0)
                    {
                        //40移至40
                        MoveBayOfContainerToContainer(strShipId, strContainerNo, strBayno, strEndContainerNo, strEndBayno, strOperareName);
                    }
                    else
                    {
                        string strBaynos = string.Empty;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            strBaynos += Convert.ToString(dt.Rows[i]["bayno"]) + "(" + Convert.ToString(dt.Rows[i]["container_no"]) + ")";
                            strBaynos += ",";
                        }
                        strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                        log.LogCatalogFailure(string.Format("待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置").DicInfo());
                    }


                }
                else if (endDtCount == 0 && frontEndDtCount_20 == 1 && backEndDtCount_20 == 0)
                {
                    string strFrontEndContainerNo_20 = Convert.ToString(frontEndDt_20.Rows[0]["container_no"]);
                    string strFrontEndUnloadMark_20 = Convert.ToString(frontEndDt_20.Rows[0]["unload_mark"]);
                    int strFrontEndSizeCon_20 = Convert.ToInt16(frontEndDt_20.Rows[0]["size_con"]);
                    if (strFrontEndSizeCon_20 == 40)
                    {
                        log.LogCatalogFailure(string.Format("船图数据有误,集装箱尺寸与实际贝位不匹配"));
                        return JsonConvert.SerializeObject(new DicPackage(false, null, "船图数据有误,集装箱尺寸与实际贝位不匹配").DicInfo());
                    }

                    if (strFrontEndUnloadMark_20.Equals("1"))
                    {
                        log.LogCatalogFailure(string.Format("目标贝位号" + strFrontEndBayno_20 + "(" + strFrontEndContainerNo_20 + ")" + "已作业"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strFrontEndBayno_20 + "(" + strFrontEndContainerNo_20 + ")" + "已作业").DicInfo());
                    }


                    //待调位置前通40
                    String strFrontBayno_40 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) - 2)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');
                    //待调位置前通20
                    String strFrontBayno_20 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) - 1)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');


                    //判断待调位置是否有被确认箱子
                    strSql =
                        string.Format(@"select bayno,container_no
                                        from con_image 
                                        where ship_id={0} and (bayno='{1}' or bayno='{2}' or bayno='{3}') and container_no <> '{4}' and unload_mark='1'",
                                        strShipId, strBayno, strFrontBayno_40, strFrontBayno_20, strContainerNo);
                    var dt = da.ExecuteTable(strSql);
                    if (dt.Rows.Count <= 0)
                    {
                        //40移至前20
                        MoveBayOfFortyToFrontTwenty(strShipId, strContainerNo, strBayno, strFrontBayno_20, strFrontEndContainerNo_20, strEndBayno, strFrontEndBayno_20, strOperareName);
                    }
                    else
                    {
                        string strBaynos = string.Empty;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            strBaynos += Convert.ToString(dt.Rows[i]["bayno"]) + "(" + Convert.ToString(dt.Rows[i]["container_no"]) + ")";
                            strBaynos += ",";
                        }
                        strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                        log.LogCatalogFailure(string.Format("待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置").DicInfo());
                    }

                    ////前位置20
                    //String strFrontBayno_20 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) - 1)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');
                    ////40移至前20
                    //MoveBayOfFortyToFrontTwenty(strShipId, strContainerNo, strBayno, strFrontBayno_20, strFrontEndContainerNo_20, strEndBayno, strFrontEndBayno_20,  strOperareName);

                }
                else if (endDtCount == 0 && frontEndDtCount_20 == 0 && backEndDtCount_20 == 1)
                {
                    string strBackEndContainerNo_20 = Convert.ToString(backEndDt_20.Rows[0]["container_no"]);
                    string strBackEndUnloadMark_20 = Convert.ToString(backEndDt_20.Rows[0]["unload_mark"]);
                    int strBackEndSizeCon_20 = Convert.ToInt16(backEndDt_20.Rows[0]["size_con"]);
                    if (strBackEndSizeCon_20 == 40)
                    {
                        log.LogCatalogFailure(string.Format("船图数据有误,集装箱尺寸与实际贝位不匹配"));
                        return JsonConvert.SerializeObject(new DicPackage(false, null, "船图数据有误,集装箱尺寸与实际贝位不匹配").DicInfo());
                    }

                    if (strBackEndUnloadMark_20.Equals("1"))
                    {
                        log.LogCatalogFailure(string.Format("目标贝位号" + strBackEndBayno_20 + "(" + strBackEndContainerNo_20 + ")" + "已作业"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strBackEndBayno_20 + "(" + strBackEndContainerNo_20 + ")" + "已作业").DicInfo());
                    }

                    //待调位置后通40
                    String strBackBayno_40 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) + 2)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');
                    //待调位置后通20
                    String strBackBayno_20 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) + 1)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');


                    //判断待调位置是否有被确认箱子
                    strSql =
                        string.Format(@"select bayno,container_no
                                        from con_image 
                                        where ship_id={0} and (bayno='{1}' or bayno='{2}' or bayno='{3}') and container_no <> '{4}' and unload_mark='1'",
                                        strShipId, strBayno, strBackBayno_40, strBackBayno_20, strContainerNo);
                    var dt = da.ExecuteTable(strSql);
                    if (dt.Rows.Count <= 0)
                    {
                        //40移至后20
                        MoveBayOfFortyToFrontTwenty(strShipId, strContainerNo, strBayno, strBackBayno_20, strBackEndContainerNo_20, strEndBayno, strBackEndBayno_20, strOperareName);
                    }
                    else
                    {
                        string strBaynos = string.Empty;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            strBaynos += Convert.ToString(dt.Rows[i]["bayno"]) + "(" + Convert.ToString(dt.Rows[i]["container_no"]) + ")";
                            strBaynos += ",";
                        }
                        strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                        log.LogCatalogFailure(string.Format("待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置").DicInfo());
                    }

                    ////后位置20
                    //String strBackBayno_20 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) + 1)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');
                    ////40移至后20
                    //MoveBayOfFortyToFrontTwenty(strShipId, strContainerNo, strBayno, strBackBayno_20, strBackEndContainerNo_20, strEndBayno, strBackEndBayno_20, strOperareName);

                }
                else if (endDtCount == 0 && frontEndDtCount_20 == 1 && backEndDtCount_20 == 1)
                {
                    string strFrontEndContainerNo_20 = Convert.ToString(frontEndDt_20.Rows[0]["container_no"]);
                    string strFrontEndUnloadMark_20 = Convert.ToString(frontEndDt_20.Rows[0]["unload_mark"]);
                    int strFrontEndSizeCon_20 = Convert.ToInt16(frontEndDt_20.Rows[0]["size_con"]);

                    string strBackEndContainerNo_20 = Convert.ToString(backEndDt_20.Rows[0]["container_no"]);
                    string strBackEndUnloadMark_20 = Convert.ToString(backEndDt_20.Rows[0]["unload_mark"]);
                    int strBackEndSizeCon_20 = Convert.ToInt16(backEndDt_20.Rows[0]["size_con"]);

                    if (strFrontEndSizeCon_20 == 40 || strBackEndSizeCon_20 == 40)
                    {
                        log.LogCatalogFailure(string.Format("船图数据有误,集装箱尺寸与实际贝位不匹配"));
                        return JsonConvert.SerializeObject(new DicPackage(false, null, "船图数据有误,集装箱尺寸与实际贝位不匹配").DicInfo());
                    }

                    if (strFrontEndUnloadMark_20.Equals("1"))
                    {
                        log.LogCatalogFailure(string.Format("目标贝位号" + strFrontEndBayno_20 + "(" + strFrontEndContainerNo_20 + ")" + "已作业"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strFrontEndBayno_20 + "(" + strFrontEndContainerNo_20 + ")" + "已作业").DicInfo());
                    }

                    if (strBackEndUnloadMark_20.Equals("1"))
                    {
                        log.LogCatalogFailure(string.Format("目标贝位号" + strBackEndBayno_20 + "(" + strBackEndContainerNo_20 + ")" + "已作业"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strBackEndBayno_20 + "(" + strBackEndContainerNo_20 + ")" + "已作业").DicInfo());
                    }

                    //待调位置前通40
                    String strFrontBayno_40 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) - 2)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');
                    //待调位置后通40
                    String strBackBayno_40 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) + 2)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');
                    //待调位置前通20
                    String strFrontBayno_20 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) - 1)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');
                    //待调位置后通20
                    String strBackBayno_20 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) + 1)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');


                    //判断待调位置是否有被确认箱子
                    strSql =
                        string.Format(@"select bayno,container_no
                                            from con_image 
                                            where ship_id={0} and (bayno='{1}' or bayno='{2}' or bayno='{3}' or bayno='{4}' or bayno='{5}') and container_no <> '{6}' and unload_mark='1'",
                                        strShipId, strBayno, strFrontBayno_40, strBackBayno_40, strFrontBayno_20, strBackBayno_20, strContainerNo);
                    var dt = da.ExecuteTable(strSql);
                    if (dt.Rows.Count <= 0)
                    {

                        MoveBayOfFortyToDoubleTwenty(strShipId, strContainerNo, strBayno, strFrontBayno_20, strBackBayno_20, strFrontEndContainerNo_20, strBackEndContainerNo_20, strEndBayno, strFrontEndBayno_20, strBackEndBayno_20, strOperareName);
                    }
                    else
                    {
                        string strBaynos = string.Empty;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            strBaynos += Convert.ToString(dt.Rows[i]["bayno"]) + "(" + Convert.ToString(dt.Rows[i]["container_no"]) + ")";
                            strBaynos += ",";
                        }
                        strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                        log.LogCatalogFailure(string.Format("待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置").DicInfo());
                    }

                    ////前位置20
                    //String strFrontBayno_20 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) - 1)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');
                    ////后位置20
                    //String strBackBayno_20 = Convert.ToString((Convert.ToInt16(strBayno.Substring(0, 2)) + 1)).PadLeft(2, '0') + strBayno.Substring(2).PadLeft(4, '0');

                    //MoveBayOfFortyToDoubleTwenty(strShipId, strContainerNo, strBayno, strFrontBayno_20, strBackBayno_20, strFrontEndContainerNo_20, strBackEndContainerNo_20, strEndBayno, strFrontEndBayno_20, strBackEndBayno_20, strOperareName);
                }
            }
            else
            {
                //前通40或后通40存在箱子，此时无法调贝
                string strBaynos = string.Empty;
                for (int i = 0; i < endDt.Rows.Count; i++)
                {
                    strBaynos += Convert.ToString(endDt.Rows[i]["bayno"]) + "(" + Convert.ToString(endDt.Rows[i]["container_no"]) + ")";
                    strBaynos += ",";
                }
                strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                log.LogCatalogFailure(string.Format("目标贝位号" + strBaynos + "存在大箱子，如需继续调贝，先将此贝位箱子移至空位置"));
                return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strBaynos + "存在大箱子，如需继续调贝，先将此贝位箱子移至空位置").DicInfo());
            }
            return null;
        }


        /// <summary>
        /// 调贝 to 空
        /// </summary>
        /// <param name="strShipId">航次ID</param>
        /// <param name="strContainerNo">待调集装箱号</param>
        /// <param name="strBayno">待调集装箱贝位号</param>
        /// <param name="strEndBayno">目标贝位号</param>
        /// <param name="strOperareName">操作人</param>
        /// <returns></returns>
        public void MoveBayOfContainerToNull(string strShipId, string strContainerNo, string strBayno, string strEndBayno, string strOperareName)
        {
            string strCurTime = DateTime.Now.ToString();
            //更新
            string strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strEndBayno, strBayno, strOperareName, strCurTime, strOperareName, strShipId, strContainerNo);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO,MARK_MOVE) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}','{6}')",
                                strShipId, strContainerNo, strBayno, strOperareName, strCurTime, strEndBayno, '1');
            da.ExecuteNonQuery(strSql);
        }

        /// <summary>
        /// 调贝（两集装箱对调,20to20,40to40）
        /// </summary>
        /// <param name="strShipId">航次ID</param>
        /// <param name="strContainerNo">待调集装箱号</param>
        /// <param name="strBayno">待调集装箱贝位号</param>
        /// <param name="strEndBayno">目标集装箱号</param>
        /// <param name="strEndBayno">目标贝位号</param>
        /// <param name="strOperareName">操作人</param>
        /// <returns></returns>
        public void MoveBayOfContainerToContainer(string strShipId, string strContainerNo, string strBayno, string strEndContainerNo, string strEndBayno, string strOperareName)
        {
            string strCurTime = DateTime.Now.ToString();


            //更新待调集装箱
            string strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strEndBayno, strBayno, strOperareName, strCurTime, strOperareName, strShipId, strContainerNo);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO,MARK_MOVE) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}','{6}')",
                                strShipId, strContainerNo, strBayno, strOperareName, strCurTime, strEndBayno, '1');
            da.ExecuteNonQuery(strSql);

            //更新目标贝位集装箱
            strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strBayno, strEndBayno, strOperareName, strCurTime, strOperareName, strShipId, strEndContainerNo);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}')",
                                strShipId, strEndContainerNo, strEndBayno, strOperareName, strCurTime, strBayno);
            da.ExecuteNonQuery(strSql);
        }


        /// <summary>
        /// 调贝（两集装箱对调,40to前20）
        /// </summary>
        /// <param name="strShipId">航次ID</param>
        /// <param name="strContainerNo">待调集装箱号</param>
        /// <param name="strBayno">待调集装箱贝位号</param>
        /// <param name="strFrontBayno_20">待调集装箱贝位号前20</param>
        /// <param name="strFrontEndContainerNo_20">目标集装箱号前20</param>
        /// <param name="strEndBayno">目标贝位号</param>
        /// <param name="strFrontEndBayno_20">目标贝位号前20</param>
        /// <param name="strOperareName">操作人</param>
        /// <returns></returns>
        /// <returns></returns>
        public void MoveBayOfFortyToFrontTwenty(string strShipId, string strContainerNo, string strBayno, string strFrontBayno_20, string strFrontEndContainerNo_20, string strEndBayno, string strFrontEndBayno_20, string strOperareName)
        {
            string strCurTime = DateTime.Now.ToString();


            //更新待调集装箱
            string strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strEndBayno, strBayno, strOperareName, strCurTime, strOperareName, strShipId, strContainerNo);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO,MARK_MOVE) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}','{6}')",
                                strShipId, strContainerNo, strBayno, strOperareName, strCurTime, strEndBayno, '1');
            da.ExecuteNonQuery(strSql);

            //更新目标贝位集装箱前20
            strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strFrontBayno_20, strFrontEndBayno_20, strOperareName, strCurTime, strOperareName, strShipId, strFrontEndContainerNo_20);
           
            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}')",
                                strShipId, strFrontEndContainerNo_20, strFrontEndBayno_20, strOperareName, strCurTime, strFrontBayno_20);
            da.ExecuteNonQuery(strSql);
        }



        /// <summary>
        /// 调贝（两集装箱对调,40to后20）
        /// </summary>
        /// <param name="strShipId">航次ID</param>
        /// <param name="strContainerNo">待调集装箱号</param>
        /// <param name="strBayno">待调集装箱贝位号</param>
        /// <param name="strBackBayno_20">待调集装箱贝位号后20</param>
        /// <param name="strBackEndContainerNo_20">目标集装箱号后20</param>
        /// <param name="strEndBayno">目标贝位号</param>
        /// <param name="strBackEndBayno_20">目标贝位号后20</param>
        /// <param name="strOperareName">操作人</param>
        /// <returns></returns>
        public void MoveBayOfFortyToDoubleTwenty(string strShipId, string strContainerNo, string strBayno,  string strBackBayno_20, string strBackEndContainerNo_20, string strEndBayno, string strBackEndBayno_20, string strOperareName)
        {


            string strCurTime = DateTime.Now.ToString();


            //更新待调集装箱
            string strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strEndBayno, strBayno, strOperareName, strCurTime, strOperareName, strShipId, strContainerNo);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO,MARK_MOVE) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}','{6}')",
                                strShipId, strContainerNo, strBayno, strOperareName, strCurTime, strEndBayno, '1');
            da.ExecuteNonQuery(strSql);

            //更新目标贝位集装箱后20
            strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strBackBayno_20, strBackEndBayno_20, strOperareName, strCurTime, strOperareName, strShipId, strBackEndContainerNo_20);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}')",
                                strShipId, strBackEndContainerNo_20, strBackEndBayno_20, strOperareName, strCurTime, strBackBayno_20);
            da.ExecuteNonQuery(strSql);
        }



        /// <summary>
        /// 调贝（40和两20对调）
        /// </summary>
        /// <param name="strShipId">航次ID</param>
        /// <param name="strContainerNo">待调集装箱号</param>
        /// <param name="strBayno">待调集装箱贝位号</param>
        /// <param name="strFrontBayno_20">待调集装箱贝位号前20</param>
        /// <param name="strBackBayno_20">待调集装箱贝位号后20</param>
        /// <param name="strFrontEndContainerNo_20">目标集装箱号前20</param>
        /// <param name="strBackEndContainerNo_20">目标集装箱号后20</param>
        /// <param name="strEndBayno">目标贝位号</param>
        /// <param name="strFrontEndBayno_20">目标贝位号前20</param>
        /// <param name="strBackEndBayno_20">目标贝位号后20</param>
        /// <param name="strOperareName">操作人</param>
        /// <returns></returns>
        public void MoveBayOfFortyToDoubleTwenty(string strShipId, string strContainerNo, string strBayno, string strFrontBayno_20, string strBackBayno_20, string strFrontEndContainerNo_20, string strBackEndContainerNo_20, string strEndBayno, string strFrontEndBayno_20, string strBackEndBayno_20, string strOperareName)
        {


            string strCurTime = DateTime.Now.ToString();


            //更新待调集装箱
            string strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strEndBayno, strBayno, strOperareName, strCurTime, strOperareName, strShipId, strContainerNo);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO,MARK_MOVE) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}','{6}')",
                                strShipId, strContainerNo, strBayno, strOperareName, strCurTime, strEndBayno, '1');
            da.ExecuteNonQuery(strSql);

            //更新目标贝位集装箱前20
            strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strFrontBayno_20, strFrontEndBayno_20, strOperareName, strCurTime, strOperareName, strShipId, strFrontEndContainerNo_20);
            
            da.ExecuteNonQuery(strSql);
           

            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}')",
                                strShipId, strFrontEndContainerNo_20, strFrontEndBayno_20, strOperareName, strCurTime, strFrontBayno_20);
            da.ExecuteNonQuery(strSql);


            //更新目标贝位集装箱后20
            strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strBackBayno_20, strBackEndBayno_20, strOperareName, strCurTime, strOperareName, strShipId, strBackEndContainerNo_20);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}')",
                                strShipId, strBackEndContainerNo_20, strBackEndBayno_20, strOperareName, strCurTime, strBackBayno_20);
            da.ExecuteNonQuery(strSql);
        }

        #endregion

        #region 双吊移贝

        /// <summary>
        ///  双吊移贝
        /// </summary>
        /// <param name="strShipId">航次ID</param>
        /// <param name="strContainerNo">待调集装箱号1</param>
        /// <param name="strBayno">待调集装箱1贝位号</param>
        /// <param name="strContainerNo">待调集装箱号2</param>
        /// <param name="strBayno">待调集装箱2贝位号</param>
        /// <param name="strEndBayno">目标贝位号（奇数）</param>
        /// <param name="strOperareName">操作人</param>
        /// <returns></returns>
        public string DoubleLift(string strShipId, string strContainerNo1, string strBayno1, string strContainerNo2, string strBayno2, string strEndBayno, string strOperareName) {

            //目标贝位前通40
            String strFrontEndBayno_40 = Convert.ToString((Convert.ToInt16(strEndBayno.Substring(0, 2)) - 1)).PadLeft(2, '0') + strEndBayno.Substring(2).PadLeft(4, '0');
            //目标贝位中通40
            String strMiddleEndBayno_40 = Convert.ToString((Convert.ToInt16(strEndBayno.Substring(0, 2)) + 1)).PadLeft(2, '0') + strEndBayno.Substring(2).PadLeft(4, '0');
            //目标贝位后通20
            String strBackEndBayno_20 = Convert.ToString((Convert.ToInt16(strEndBayno.Substring(0, 2)) + 2)).PadLeft(2, '0') + strEndBayno.Substring(2).PadLeft(4, '0');
            //目标贝位后通40
            String strBackEndBayno_40 = Convert.ToString((Convert.ToInt16(strEndBayno.Substring(0, 2)) + 3)).PadLeft(2, '0') + strEndBayno.Substring(2).PadLeft(4, '0');

            //判断是否有通贝40箱子，有，不能调贝
            string strSql =
                string.Format(@"select t.size_con,t.bayno,t.container_no 
                                from con_image t 
                                where ship_id={0} and (bayno = '{1}' or bayno = '{2}')",
                                strShipId, strFrontEndBayno_40, strBackEndBayno_40);
            var endDt = da.ExecuteTable(strSql);
            if (endDt.Rows.Count <= 0)
            {
                strSql =
                    string.Format(@"select t.size_con,t.bayno,t.container_no,t.unload_mark 
                                    from con_image t 
                                    where ship_id={0} and bayno = '{1}'",
                                    strShipId, strMiddleEndBayno_40);
                var middleEndDt_40  = da.ExecuteTable(strSql);

                strSql =
                     string.Format(@"select t.size_con,t.bayno,t.container_no,t.unload_mark 
                                    from con_image t 
                                    where ship_id={0} and bayno = '{1}'",
                                    strShipId, strEndBayno);
                endDt = da.ExecuteTable(strSql);

                strSql =
                     string.Format(@"select t.size_con,t.bayno,t.container_no,t.unload_mark 
                                    from con_image t 
                                    where ship_id={0} and bayno = '{1}'",
                                    strShipId, strBackEndBayno_20); 
                var backEndDt_20 = da.ExecuteTable(strSql);

                /************校验是否存在多箱**************/
                int endDtCount = endDt.Rows.Count;
                int middleEndDtCount_40 = middleEndDt_40.Rows.Count;
                int backEndDtCount_20 = backEndDt_20.Rows.Count;

                if (middleEndDtCount_40 > 1)
                {

                    //目标位置中通40存在多箱，此时无法调贝
                    string strBaynos = string.Empty;
                    for (int i = 0; i < middleEndDtCount_40; i++)
                    {
                        strBaynos += Convert.ToString(middleEndDt_40.Rows[i]["bayno"]) + "(" + Convert.ToString(middleEndDt_40.Rows[i]["container_no"]) + ")";
                        strBaynos += ",";
                    }
                    strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                    log.LogCatalogFailure(string.Format("目标贝位号" + strBaynos + "存在多个大箱子，如需继续调贝，先将此贝位箱子移至空位置"));
                    return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strBaynos + "存在多个大箱子，如需继续调贝，先将此贝位箱子移至空位置").DicInfo());
                }

                if (endDtCount > 1)
                {

                    //目标贝位存在多箱，此时无法调贝
                    string strBaynos = string.Empty;
                    for (int i = 0; i < endDtCount; i++)
                    {
                        strBaynos += Convert.ToString(endDt.Rows[i]["bayno"]) + "(" + Convert.ToString(endDt.Rows[i]["container_no"]) + ")";
                        strBaynos += ",";
                    }
                    strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                    log.LogCatalogFailure(string.Format("目标贝位号" + strBaynos + "存在多个小箱子，如需继续调贝，先将此贝位箱子移至空位置"));
                    return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strBaynos + "存在多个小箱子，如需继续调贝，先将此贝位箱子移至空位置").DicInfo());
                }


                if (backEndDtCount_20 > 1)
                {

                    //目标贝位后20位置存在多箱，此时无法调贝
                    string strBaynos = string.Empty;
                    for (int i = 0; i < backEndDtCount_20; i++)
                    {
                        strBaynos += Convert.ToString(backEndDt_20.Rows[i]["bayno"]) + "(" + Convert.ToString(backEndDt_20.Rows[i]["container_no"]) + ")";
                        strBaynos += ",";
                    }
                    strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                    log.LogCatalogFailure(string.Format("目标贝位号" + strBaynos + "存在多个小箱子，如需继续调贝，先将此贝位箱子移至空位置"));
                    return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strBaynos + "存在多个小箱子，如需继续调贝，先将此贝位箱子移至空位置").DicInfo());
                }

                if ((middleEndDtCount_40 == 1 && endDtCount == 1 && backEndDtCount_20 == 1) || (middleEndDtCount_40 == 1 && endDtCount == 1) || (middleEndDtCount_40 == 1 && backEndDtCount_20 == 1))
                {


                    //目标贝位、目标贝位中40位置、目标贝位后20位置同时存在一个箱子，此时无法调贝
                    string strBaynos = string.Empty;
                    for (int i = 0; i < middleEndDtCount_40; i++)
                    {
                        strBaynos += Convert.ToString(middleEndDt_40.Rows[i]["bayno"]) + "(" + Convert.ToString(middleEndDt_40.Rows[i]["container_no"]) + ")";
                        strBaynos += ",";
                    }

                    for (int i = 0; i < endDtCount; i++)
                    {
                        strBaynos += Convert.ToString(endDt.Rows[i]["bayno"]) + "(" + Convert.ToString(endDt.Rows[i]["container_no"]) + ")";
                        strBaynos += ",";
                    }

                    for (int i = 0; i < backEndDtCount_20; i++)
                    {
                        strBaynos += Convert.ToString(backEndDt_20.Rows[i]["bayno"]) + "(" + Convert.ToString(backEndDt_20.Rows[i]["container_no"]) + ")";
                        strBaynos += ",";
                    }

                    strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                    log.LogCatalogFailure(string.Format("目标贝位号" + strBaynos + "同时存在箱子，如需继续调贝，先将此贝位箱子移至空位置"));
                    return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strBaynos + "同时存在箱子，如需继续调贝，先将此贝位箱子移至空位置").DicInfo());
                }



                /************调贝**************/
                if (middleEndDtCount_40 == 0 && endDtCount == 0 && backEndDtCount_20 == 0)
                {
                    //双吊移至空
                    DoubleLiftOfDoubleTwentyToNull(strShipId, strContainerNo1, strBayno1, strContainerNo2, strBayno2, strEndBayno, strBackEndBayno_20, strOperareName);
                }
                else if (middleEndDtCount_40 == 1 && endDtCount == 0 && backEndDtCount_20 == 0)
                {
                    //目标贝位中通40存在一个箱子
                    string strMiddleEndContainerNo = Convert.ToString(middleEndDt_40.Rows[0]["container_no"]);
                    string strMiddleEndUnloadMark = Convert.ToString(middleEndDt_40.Rows[0]["unload_mark"]);
                    int strMiddleEndSizeCon = Convert.ToInt16(middleEndDt_40.Rows[0]["size_con"]);
                    if (strMiddleEndSizeCon == 20)
                    {
                        log.LogCatalogFailure(string.Format("船图数据有误,集装箱尺寸与实际贝位不匹配"));
                        return JsonConvert.SerializeObject(new DicPackage(false, null, "船图数据有误,集装箱尺寸与实际贝位不匹配").DicInfo());
                    }

                    if (strMiddleEndUnloadMark.Equals("1"))
                    {
                        log.LogCatalogFailure(string.Format("目标贝位号" + strMiddleEndBayno_40 + "(" + strMiddleEndContainerNo + ")" + "已作业"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strMiddleEndBayno_40 + "(" + strMiddleEndContainerNo + ")" + "已作业").DicInfo());
                    }

                    //校验待调集装箱1和集装箱号2是否通贝
                    //待调位置后通20
                    String strBackBayno_20 = Convert.ToString((Convert.ToInt16(strBayno1.Substring(0, 2)) + 2)).PadLeft(2, '0') + strBayno1.Substring(2).PadLeft(4, '0');
                    if (!strBackBayno_20.Equals(strBayno2))
                    {
                        //不通贝            
                        log.LogCatalogFailure(string.Format("目标贝位号" + strMiddleEndBayno_40 + "(" + strMiddleEndContainerNo + ")" + "存在大箱子" + "，但待调两个集装箱不通贝"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strMiddleEndBayno_40 + "(" + strMiddleEndContainerNo + ")" + "是大箱子" + "，但待调两个集装箱不通贝").DicInfo());

                    }
        

                    //待调位置前通40
                    String strFrontBayno_40 = Convert.ToString((Convert.ToInt16(strBayno1.Substring(0, 2)) - 1)).PadLeft(2, '0') + strBayno1.Substring(2).PadLeft(4, '0');
                    //待调位置中通40
                    String strMiddleBayno_40 = Convert.ToString((Convert.ToInt16(strBayno1.Substring(0, 2)) + 1)).PadLeft(2, '0') + strBayno1.Substring(2).PadLeft(4, '0');
                    //待调位置后通40
                    String strBackBayno_40 = Convert.ToString((Convert.ToInt16(strBayno2.Substring(0, 2)) + 1)).PadLeft(2, '0') + strBayno1.Substring(2).PadLeft(4, '0');


                    //判断待调位置是否有被确认箱子
                    strSql =
                        string.Format(@"select bayno,container_no
                                        from con_image 
                                        where ship_id={0} and (bayno='{1}' or bayno='{2}' or bayno='{3}' or bayno='{4}' or bayno='{5}') and container_no <> '{6}' and container_no <> '{7}' and unload_mark='1'",
                                        strShipId, strBayno1, strBayno2, strFrontBayno_40, strMiddleBayno_40, strBackBayno_40, strContainerNo1, strContainerNo2);
                    var dt = da.ExecuteTable(strSql);
                    if (dt.Rows.Count <= 0)
                    {
                        //双吊移至40
                        DoubleLiftOfDoubleTwentyToForty(strShipId, strContainerNo1, strBayno1, strContainerNo2, strBayno2, strMiddleBayno_40, strMiddleEndContainerNo, strEndBayno, strBackEndBayno_20, strMiddleEndBayno_40,  strOperareName);
                    }
                    else
                    {
                        string strBaynos = string.Empty;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            strBaynos += Convert.ToString(dt.Rows[i]["bayno"]) + "(" + Convert.ToString(dt.Rows[i]["container_no"]) + ")";
                            strBaynos += ",";
                        }
                        strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                        log.LogCatalogFailure(string.Format("待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置").DicInfo());
                    }

                }
                else if (middleEndDtCount_40 == 0 && endDtCount ==1 && backEndDtCount_20 == 0)
                {
                   
                    //目标贝位存在一个箱子
                    string strEndContainerNo_20 = Convert.ToString(endDt.Rows[0]["container_no"]);
                    string strEndUnloadMark_20 = Convert.ToString(endDt.Rows[0]["unload_mark"]);
                    int strEndSizeCon_20 = Convert.ToInt16(endDt.Rows[0]["size_con"]);
                    if (strEndSizeCon_20 == 40)
                    {
                        log.LogCatalogFailure(string.Format("船图数据有误,集装箱尺寸与实际贝位不匹配"));
                        return JsonConvert.SerializeObject(new DicPackage(false, null, "船图数据有误,集装箱尺寸与实际贝位不匹配").DicInfo());
                    }

                    if (strEndUnloadMark_20.Equals("1"))
                    {
                        log.LogCatalogFailure(string.Format("目标贝位号" + strEndBayno + "(" + strEndContainerNo_20 + ")" + "已作业"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strEndBayno + "(" + strEndContainerNo_20 + ")" + "已作业").DicInfo());
                    }


                    //待调位置前通40
                    String strFrontBayno_40 = Convert.ToString((Convert.ToInt16(strBayno1.Substring(0, 2)) - 1)).PadLeft(2, '0') + strBayno1.Substring(2).PadLeft(4, '0');
                    //待调位置中通40
                    String strMiddleBayno_40 = Convert.ToString((Convert.ToInt16(strBayno1.Substring(0, 2)) + 1)).PadLeft(2, '0') + strBayno1.Substring(2).PadLeft(4, '0');


                    //判断待调位置是否有被确认箱子
                    strSql =
                        string.Format(@"select bayno,container_no
                                        from con_image 
                                        where ship_id={0} and (bayno='{1}' or bayno='{2}' or bayno='{3}') and container_no <> '{4}' and unload_mark='1'",
                                        strShipId, strBayno1, strFrontBayno_40, strMiddleBayno_40, strContainerNo1);
                    var dt = da.ExecuteTable(strSql);
                    if (dt.Rows.Count <= 0)
                    {
                        //双吊移至前20
                        DoubleLiftOfDoubleTwentyToFrontTwenty(strShipId, strContainerNo1, strBayno1, strContainerNo2, strBayno2, strEndContainerNo_20, strEndBayno, strBackEndBayno_20, strOperareName);
                    }
                    else
                    {
                        string strBaynos = string.Empty;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            strBaynos += Convert.ToString(dt.Rows[i]["bayno"]) + "(" + Convert.ToString(dt.Rows[i]["container_no"]) + ")";
                            strBaynos += ",";
                        }
                        strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                        log.LogCatalogFailure(string.Format("待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置").DicInfo());
                    }

                }
                else if (middleEndDtCount_40 == 0 && endDtCount == 0 && backEndDtCount_20 == 1)
                {
                    //目标贝位后通20存在一个箱子
                    string strBackEndContainerNo_20 = Convert.ToString(backEndDt_20.Rows[0]["container_no"]);
                    string strBackEndUnloadMark_20 = Convert.ToString(backEndDt_20.Rows[0]["unload_mark"]);
                    int strBackEndSizeCon_20 = Convert.ToInt16(backEndDt_20.Rows[0]["size_con"]);
                    if (strBackEndSizeCon_20 == 40)
                    {
                        log.LogCatalogFailure(string.Format("船图数据有误,集装箱尺寸与实际贝位不匹配"));
                        return JsonConvert.SerializeObject(new DicPackage(false, null, "船图数据有误,集装箱尺寸与实际贝位不匹配").DicInfo());
                    }

                    if (strBackEndUnloadMark_20.Equals("1"))
                    {
                        log.LogCatalogFailure(string.Format("目标贝位号" + strBackEndBayno_20 + "(" + strBackEndContainerNo_20 + ")" + "已作业"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strBackEndBayno_20 + "(" + strBackEndContainerNo_20 + ")" + "已作业").DicInfo());
                    }


                    //待调位置中通40
                    String strMiddleBayno_40 = Convert.ToString((Convert.ToInt16(strBayno1.Substring(0, 2)) + 1)).PadLeft(2, '0') + strBayno1.Substring(2).PadLeft(4, '0');
                    //待调位置后通40
                    String strBackBayno_40 = Convert.ToString((Convert.ToInt16(strBayno2.Substring(0, 2)) + 1)).PadLeft(2, '0') + strBayno1.Substring(2).PadLeft(4, '0');


                    //判断待调位置是否有被确认箱子
                    strSql =
                        string.Format(@"select bayno,container_no
                                        from con_image 
                                        where ship_id={0} and (bayno='{1}' or bayno='{2}' or bayno='{3}') and container_no <> '{4}' and unload_mark='1'",
                                        strShipId, strBayno2, strMiddleBayno_40, strBackBayno_40, strContainerNo2);
                    var dt = da.ExecuteTable(strSql);
                    if (dt.Rows.Count <= 0)
                    {
                        //双吊移至后20
                        DoubleLiftOfDoubleTwentyToBackTwenty(strShipId, strContainerNo1, strBayno1, strContainerNo2, strBayno2, strBackEndContainerNo_20, strEndBayno, strBackEndBayno_20, strOperareName);
                    }
                    else
                    {
                        string strBaynos = string.Empty;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            strBaynos += Convert.ToString(dt.Rows[i]["bayno"]) + "(" + Convert.ToString(dt.Rows[i]["container_no"]) + ")";
                            strBaynos += ",";
                        }
                        strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                        log.LogCatalogFailure(string.Format("待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置").DicInfo());
                    }
                }
                else if (middleEndDtCount_40 == 0 && endDtCount == 1 && backEndDtCount_20 == 1)
                {
                    //目标贝位和后通20同时存在一个箱子
                    string strEndContainerNo_20 = Convert.ToString(endDt.Rows[0]["container_no"]);
                    string strEndUnloadMark_20 = Convert.ToString(endDt.Rows[0]["unload_mark"]);
                    int strEndSizeCon_20 = Convert.ToInt16(endDt.Rows[0]["size_con"]);

                    string strBackEndContainerNo_20 = Convert.ToString(backEndDt_20.Rows[0]["container_no"]);
                    string strBackEndUnloadMark_20 = Convert.ToString(backEndDt_20.Rows[0]["unload_mark"]);
                    int strBackEndSizeCon_20 = Convert.ToInt16(backEndDt_20.Rows[0]["size_con"]);

                    if (strEndSizeCon_20 == 40 || strBackEndSizeCon_20 == 40)
                    {
                        log.LogCatalogFailure(string.Format("船图数据有误,集装箱尺寸与实际贝位不匹配"));
                        return JsonConvert.SerializeObject(new DicPackage(false, null, "船图数据有误,集装箱尺寸与实际贝位不匹配").DicInfo());
                    }

                    if (strEndUnloadMark_20.Equals("1"))
                    {
                        log.LogCatalogFailure(string.Format("目标贝位号" + strEndBayno + "(" + strEndContainerNo_20 + ")" + "已作业"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strEndBayno + "(" + strEndContainerNo_20 + ")" + "已作业").DicInfo());
                    }

                    if (strBackEndUnloadMark_20.Equals("1"))
                    {
                        log.LogCatalogFailure(string.Format("目标贝位号" + strBackEndBayno_20 + "(" + strBackEndContainerNo_20 + ")" + "已作业"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strBackEndBayno_20 + "(" + strBackEndContainerNo_20 + ")" + "已作业").DicInfo());
                    }


                    //待调位置前通40
                    String strFrontBayno_40 = Convert.ToString((Convert.ToInt16(strBayno1.Substring(0, 2)) - 1)).PadLeft(2, '0') + strBayno1.Substring(2).PadLeft(4, '0');
                    //待调位置中通40
                    String strMiddleBayno_40 = Convert.ToString((Convert.ToInt16(strBayno1.Substring(0, 2)) + 1)).PadLeft(2, '0') + strBayno1.Substring(2).PadLeft(4, '0');
                    //待调位置后通40
                    String strBackBayno_40 = Convert.ToString((Convert.ToInt16(strBayno2.Substring(0, 2)) + 1)).PadLeft(2, '0') + strBayno1.Substring(2).PadLeft(4, '0');


                    //判断待调位置是否有被确认箱子
                    strSql =
                        string.Format(@"select bayno,container_no
                                        from con_image 
                                        where ship_id={0} and (bayno='{1}' or bayno='{2}' or bayno='{3}' or bayno='{4}' or bayno='{5}') and container_no <> '{6}' and container_no <> '{7}' and unload_mark='1'",
                                        strShipId, strBayno1, strBayno2, strFrontBayno_40, strMiddleBayno_40, strBackBayno_40,  strContainerNo1, strContainerNo2);
                    var dt = da.ExecuteTable(strSql);
                    if (dt.Rows.Count <= 0)
                    {
                        //双吊移至双20
                        DoubleLiftOfDoubleTwentyToDoubleTwenty(strShipId, strContainerNo1, strBayno1, strContainerNo2, strBayno2, strEndContainerNo_20, strBackEndContainerNo_20, strEndBayno, strBackEndBayno_20, strOperareName);
                    }
                    else
                    {
                        string strBaynos = string.Empty;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            strBaynos += Convert.ToString(dt.Rows[i]["bayno"]) + "(" + Convert.ToString(dt.Rows[i]["container_no"]) + ")";
                            strBaynos += ",";
                        }
                        strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                        log.LogCatalogFailure(string.Format("待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置"));
                        return JsonConvert.SerializeObject(new DicPackage(true, null, "待调贝位号" + strBaynos + "箱子已被确认，如需继续调贝，先将目标贝位箱子移至空位置").DicInfo());
                    }
                }
            }
            else
            {
                //前通40或后通40存在箱子，此时无法调贝
                string strBaynos = string.Empty;
                for (int i = 0; i < endDt.Rows.Count; i++)
                {
                    strBaynos += Convert.ToString(endDt.Rows[i]["bayno"]) + "(" + Convert.ToString(endDt.Rows[i]["container_no"]) + ")";
                    strBaynos += ",";
                }
                strBaynos = strBaynos.Substring(0, strBaynos.Length - 1);

                log.LogCatalogFailure(string.Format("目标贝位号" + strBaynos + "存在大箱子，如需继续调贝，先将此贝位箱子移至空位置"));
                return JsonConvert.SerializeObject(new DicPackage(true, null, "目标贝位号" + strBaynos + "存在大箱子，如需继续调贝，先将此贝位箱子移至空位置").DicInfo());
            }
            return null;
        }

        /// <summary>
        /// 双吊 to 空
        /// </summary>
        /// <param name="strShipId">航次ID</param>
        /// <param name="strContainerNo1">待调集装箱号1</param>
        /// <param name="strBayno1">待调集装箱1贝位号</param>
        /// <param name="strContainerNo2">待调集装箱号2</param>
        /// <param name="strBayno2">待调集装箱2贝位号</param>
        /// <param name="strEndBayno">目标贝位（奇数）</param>
        /// <param name="strBackEndBayno_20">目标位置后通20</param>
        /// <param name="strOperareName">操作人</param>
        public void DoubleLiftOfDoubleTwentyToNull(string strShipId, string strContainerNo1, string strBayno1, string strContainerNo2, string strBayno2, string strEndBayno, string strBackEndBayno_20, string strOperareName)
        {

            string strCurTime = DateTime.Now.ToString();
            //更新集装箱1
            string strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strEndBayno, strBayno1, strOperareName, strCurTime, strOperareName, strShipId, strContainerNo1);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO,MARK_MOVE) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}','{6}')",
                                strShipId, strContainerNo1, strBayno1, strOperareName, strCurTime, strEndBayno, '1');
            da.ExecuteNonQuery(strSql);

            //更新集装箱2
            strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strBackEndBayno_20, strBayno2, strOperareName, strCurTime, strOperareName, strShipId, strContainerNo2);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO,MARK_MOVE) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}','{6}')",
                                strShipId, strContainerNo2, strBayno2, strOperareName, strCurTime, strBackEndBayno_20, '1');
            da.ExecuteNonQuery(strSql);

        }

        /// <summary>
        /// 双吊 to 前20
        /// </summary>
        /// <param name="strShipId">航次ID</param>
        /// <param name="strContainerNo1">集装箱号1</param>
        /// <param name="strBayno1">待调集装箱1贝位号</param>
        /// <param name="strContainerNo2">集装箱号2</param>
        /// <param name="strBayno2">待调集装箱2贝位号</param>
        /// <param name="strEndContainerNo_20">目标贝位集装箱号</param>
        /// <param name="strEndBayno">目标贝位号（奇数）</param>
        /// <param name="strBackEndBayno_20">目标贝位后通20</param>
        /// <param name="strOperareName">操作人</param>
        public void DoubleLiftOfDoubleTwentyToFrontTwenty(string strShipId, string strContainerNo1, string strBayno1, string strContainerNo2, string strBayno2, string strEndContainerNo_20, string strEndBayno, string strBackEndBayno_20, string strOperareName)
        {
            string strCurTime = DateTime.Now.ToString();
            //更新集装箱1
            string strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strEndBayno, strBayno1, strOperareName, strCurTime, strOperareName, strShipId, strContainerNo1);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO,MARK_MOVE) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}','{6}')",
                                strShipId, strContainerNo1, strBayno1, strOperareName, strCurTime, strEndBayno, '1');
            da.ExecuteNonQuery(strSql);

            //更新集装箱2
            strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strBackEndBayno_20, strBayno2, strOperareName, strCurTime, strOperareName, strShipId, strContainerNo2);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO,MARK_MOVE) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}','{6}')",
                                strShipId, strContainerNo2, strBayno2, strOperareName, strCurTime, strBackEndBayno_20, '1');
            da.ExecuteNonQuery(strSql);


            //更新目标贝位集装箱
            strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strBayno1, strEndBayno, strOperareName, strCurTime, strOperareName, strShipId, strEndContainerNo_20);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}')",
                                strShipId, strEndContainerNo_20, strEndBayno, strOperareName, strCurTime, strBayno1);
            da.ExecuteNonQuery(strSql);
        }

        /// <summary>
        /// 双吊 to 后20
        /// </summary>
        /// <param name="strShipId">航次ID</param>
        /// <param name="strContainerNo1">集装箱号1</param>
        /// <param name="strBayno1">待调集装箱1贝位号</param>
        /// <param name="strContainerNo2">集装箱号2</param>
        /// <param name="strBayno2">待调集装箱2贝位号</param>
        /// <param name="strBackEndContainerNo_20">目标贝位后通20集装箱号</param>
        /// <param name="strEndBayno">目标贝位号（奇数）</param>
        /// <param name="strBackEndBayno_20">目标贝位后通20</param>
        /// <param name="strOperareName">操作人</param>
        public void DoubleLiftOfDoubleTwentyToBackTwenty(string strShipId, string strContainerNo1, string strBayno1, string strContainerNo2, string strBayno2, string strBackEndContainerNo_20, string strEndBayno, string strBackEndBayno_20, string strOperareName)
        {
            string strCurTime = DateTime.Now.ToString();
            //更新集装箱1
            string strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strEndBayno, strBayno1, strOperareName, strCurTime, strOperareName, strShipId, strContainerNo1);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO,MARK_MOVE) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}','{6}')",
                                strShipId, strContainerNo1, strBayno1, strOperareName, strCurTime, strEndBayno, '1');
            da.ExecuteNonQuery(strSql);

            //更新集装箱2
            strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strBackEndBayno_20, strBayno2, strOperareName, strCurTime, strOperareName, strShipId, strContainerNo2);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO,MARK_MOVE) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}','{6}')",
                                strShipId, strContainerNo2, strBayno2, strOperareName, strCurTime, strBackEndBayno_20, '1');
            da.ExecuteNonQuery(strSql);


            //更新目标贝位后通20集装箱
            strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strBayno2, strBackEndBayno_20, strOperareName, strCurTime, strOperareName, strShipId, strBackEndContainerNo_20);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}')",
                                strShipId, strBackEndContainerNo_20, strBackEndBayno_20, strOperareName, strCurTime, strBayno2);
            da.ExecuteNonQuery(strSql);
        }

        /// <summary>
        /// 双吊 to 双20
        /// </summary>
        /// <param name="strShipId">航次ID</param>
        /// <param name="strContainerNo1">集装箱号1</param>
        /// <param name="strBayno1">待调集装箱1贝位号</param>
        /// <param name="strContainerNo2">集装箱号2</param>
        /// <param name="strBayno2">待调集装箱2贝位号</param>
        /// <param name="strEndContainerNo_20">目标贝位集装箱号</param>
        /// <param name="strBackEndContainerNo_20">目标贝位后通20集装箱号</param>
        /// <param name="strEndBayno">目标贝位号（奇数）</param>
        /// <param name="strBackEndBayno_20">目标贝位后通20</param>
        /// <param name="strOperareName">操作人</param>
        public void DoubleLiftOfDoubleTwentyToDoubleTwenty(string strShipId, string strContainerNo1, string strBayno1, string strContainerNo2, string strBayno2, string strEndContainerNo_20, string strBackEndContainerNo_20, string strEndBayno, string strBackEndBayno_20, string strOperareName)
        {
            string strCurTime = DateTime.Now.ToString();
            //更新集装箱1
            string strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strEndBayno, strBayno1, strOperareName, strCurTime, strOperareName, strShipId, strContainerNo1);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO,MARK_MOVE) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}','{6}')",
                                strShipId, strContainerNo1, strBayno1, strOperareName, strCurTime, strEndBayno, '1');
            da.ExecuteNonQuery(strSql);

            //更新集装箱2
            strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strBackEndBayno_20, strBayno2, strOperareName, strCurTime, strOperareName, strShipId, strContainerNo2);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO,MARK_MOVE) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}','{6}')",
                                strShipId, strContainerNo2, strBayno2, strOperareName, strCurTime, strBackEndBayno_20, '1');
            da.ExecuteNonQuery(strSql);


            //更新目标贝位集装箱
            strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strBayno1, strEndBayno, strOperareName, strCurTime, strOperareName, strShipId, strEndContainerNo_20);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}')",
                                strShipId, strEndContainerNo_20, strEndBayno, strOperareName, strCurTime, strBayno1);
            da.ExecuteNonQuery(strSql);


            //更新目标贝位后通20集装箱
            strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strBayno2, strBackEndBayno_20, strOperareName, strCurTime, strOperareName, strShipId, strBackEndContainerNo_20);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}')",
                                strShipId, strBackEndContainerNo_20, strBackEndBayno_20, strOperareName, strCurTime, strBayno2);
            da.ExecuteNonQuery(strSql);
        }

        /// <summary>
        /// 双吊 to 40
        /// </summary>
        /// <param name="strShipId">航次ID</param>
        /// <param name="strContainerNo1">集装箱号1</param>
        /// <param name="strBayno1">待调集装箱1贝位号</param>
        /// <param name="strContainerNo2">集装箱号2</param>
        /// <param name="strBayno2">待调集装箱2贝位号</param>
        /// <param name="strMiddleBayno_40">待调贝位1中通40贝位号</param>
        /// <param name="strMiddleEndContainerNo">目标贝位中通40集装箱号</param>
        /// <param name="strEndBayno">目标贝位号（奇数）</param>
        /// <param name="strBackEndContainerNo_20">目标贝位后通20集装箱号</param>
        /// <param name="strMiddleEndBayno_40">目标贝位中通40贝位号</param>
        /// <param name="strOperareName">操作人</param>
        public void DoubleLiftOfDoubleTwentyToForty(string strShipId, string strContainerNo1, string strBayno1, string strContainerNo2, string strBayno2, string strMiddleBayno_40, string strMiddleEndContainerNo, string strEndBayno, string strBackEndBayno_20, string strMiddleEndBayno_40, string strOperareName)
        {
            string strCurTime = DateTime.Now.ToString();
            //更新集装箱1
            string strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strEndBayno, strBayno1, strOperareName, strCurTime, strOperareName, strShipId, strContainerNo1);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO,MARK_MOVE) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}','{6}')",
                                strShipId, strContainerNo1, strBayno1, strOperareName, strCurTime, strEndBayno, '1');
            da.ExecuteNonQuery(strSql);

            //更新集装箱2
            strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strBackEndBayno_20, strBayno2, strOperareName, strCurTime, strOperareName, strShipId, strContainerNo2);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO,MARK_MOVE) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}','{6}')",
                                strShipId, strContainerNo2, strBayno2, strOperareName, strCurTime, strBackEndBayno_20, '1');
            da.ExecuteNonQuery(strSql);

            //更新目标贝位中通40集装箱
            strSql =
                string.Format(@"update CON_IMAGE 
                                set BAYNO='{0}',OLDBAYNO='{1}',BAY_OPERNAME='{2}',BAY_OPERTIME=to_date('{3}', 'yyyy-mm-dd HH24:mi:ss'),USER_NAME='{4}'
                                where SHIP_ID='{5}' and CONTAINER_NO='{6}'",
                                strMiddleBayno_40, strMiddleEndBayno_40, strOperareName, strCurTime, strOperareName, strShipId, strMiddleEndContainerNo);

            da.ExecuteNonQuery(strSql);


            //保留原船图数据记录
            strSql =
                string.Format(@"insert into TB_APP_TALLY_LOG (SHIP_ID,CONTAINER_NO,BAYNO,BAY_OPERNAME,BAY_OPERTIME,MOVEDBAYNO) 
                                values('{0}','{1}','{2}','{3}',to_date('{4}','yyyy/mm/dd HH24:mi:ss'),'{5}')",
                                strShipId, strMiddleEndContainerNo, strMiddleEndBayno_40, strOperareName, strCurTime, strMiddleBayno_40);
            da.ExecuteNonQuery(strSql);
        }

        #endregion

    }
}