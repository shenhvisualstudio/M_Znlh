//
//文件名：    ShipImageE.aspx.cs
//功能描述：  船图数据集
//创建时间：  2017/1/20
//作者：      
//修改时间：  暂无
//修改描述：  暂无
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace M_Znlh.Ship
{
    public class ShipImageE
    {

        /// <summary>
        /// 船图
        /// </summary>
        public struct ShipImage {

            //编码ID
            public string id { get; set; }

            //航次编码
            public string ship_id { get; set; }

            //船舶编码
            public string v_id { get; set; }

            //英文船名
            public string eng_vessel { get; set; }
            
            //中文船名
            public string chi_vessel { get; set; }

            //甲板/舱内
            public string location { get; set; }

            //贝号
            public string bay_num { get; set; }

            //贝列
            public string bay_col { get; set; }

            //贝层
            public string bay_row { get; set; }

            //标准贝位
            public string sbayno { get; set; }

            //理论通贝
            public string tbayno { get; set; }

            //被通时贝位
            public string jbayno { get; set; }

            //有贝标志
            public string user_char { get; set; }

            //屏幕行
            public int screen_row { get; set; }

            //屏幕列
            public int screen_col { get; set; }

            //通贝标志
            public string joint { get; set; }

            //装货港
            public string code_load_port { get; set; }

            //卸货港
            public string code_unload_port { get; set; }

            //交界地
            public string delivery { get; set; }

            //捣箱标志
            public string moved { get; set; }

            //卸箱标志（作业标志）
            public string unload_mark { get; set; }

            //理货员工号
            public string work_no { get; set; }

            //危险品等级
            public string danger_grade { get; set; }

            //设置温度
            public string degree_setting { get; set; }

            //温度单位
            public string degree_unit { get; set; }

            //最小温度
            public string min_degree { get; set; }

            //最大温度
            public string max_degree { get; set; }

            //实际贝位号
            public string bayno { get; set; }

            //原贝
            public string oldbayno { get; set; }

            //桥吊号
            public string code_crane { get; set; }

            //船图ID
            public string image_id { get; set; }

            //贝
            public string baynum { get; set; }

             //列
            public string baycol { get; set; }

            //层
            public string bayrow { get; set; }

            //箱号
            public string container_no { get; set; }

            //尺寸
            public string size_con { get; set; }

            /**
            //箱型
             */
            public string container_type { get; set; }

            //空/重
            public string code_empty { get; set; }

            //重量
            public string weight { get; set; }

            //工作时间
            public string work_date { get; set; }

            //铅封号
            public string sealno { get; set; }

            //捣箱
            public string moved_name { get; set; }

            //内外贸
            public string inoutmark { get; set; }

            //中转箱
            public string transmark { get; set; }

            //节假日
            public string holidays { get; set; }

            //夜班
            public string night { get; set; }
            //理货员

            public string name { get; set; }

            //数据修改标志
            public string mark_modify { get; set; }

            //修改人（工号）
            public string modifier { get; set; }

            //修改时间
            public string modifytime { get; set; }
        }
    }
}