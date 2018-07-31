using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace M_Znlh.Statistics
{
    public class FullStatisticsE
    {

        /// <summary>
        /// 预配合计
        /// </summary>
        private int forecast_total = 0;
        /// <summary>
        /// 预配空20
        /// </summary>
        private int forecast_E_20 = 0;
        /// <summary>
        /// 预配空40
        /// </summary>
        private int forecast_E_40 = 0;
        /// <summary>
        /// 预配空其他
        /// </summary>
        private int forecast_E_other = 0;
        /// <summary>
        /// 预配空合计
        /// </summary>
        private int forecast_E_total = 0;
        /// <summary>
        /// 预配重20
        /// </summary>
        private int forecast_F_20 = 0;
        /// <summary>
        /// 预配重40
        /// </summary>
        private int forecast_F_40 = 0;
        /// <summary>
        /// 预配重其他
        /// </summary>
        private int forecast_F_other = 0;
        /// <summary>
        /// 预配重合计
        /// </summary>
        private int forecast_F_total = 0;


        /// <summary>
        /// 理货合计
        /// </summary>
        private int tally_total = 0;
        /// <summary>
        /// 理货空20
        /// </summary>
        private int tally_E_20 = 0;
        /// <summary>
        /// 理货空40
        /// </summary>
        private int tally_E_40 = 0;
        /// <summary>
        /// 理货空其他
        /// </summary>
        private int tally_E_other = 0;
        /// <summary>
        /// 理货空合计
        /// </summary>
        private int tally_E_total = 0;
        /// <summary>
        /// 理货重20
        /// </summary>
        private int tally_F_20 = 0;
        /// <summary>
        /// 理货重40
        /// </summary>
        private int tally_F_40 = 0;
        /// <summary>
        /// 理货重其他
        /// </summary>
        private int tally_F_other = 0;
        /// <summary>
        /// 理货重合计
        /// </summary>
        private int tally_F_total = 0;


        /// <summary>
        /// 异常合计
        /// </summary>
        private int abnormal_total = 0;
        /// <summary>
        /// 异常空20
        /// </summary>
        private int abnormal_E_20 = 0;
        /// <summary>
        /// 异常空40
        /// </summary>
        private int abnormal_E_40 = 0;
        /// <summary>
        /// 异常空其他
        /// </summary>
        private int abnormal_E_other = 0;
        /// <summary>
        /// 异常空合计
        /// </summary>
        private int abnormal_E_total = 0;
        /// <summary>
        /// 异常重20
        /// </summary>
        private int abnormal_F_20 = 0;
        /// <summary>
        /// 异常重40
        /// </summary>
        private int abnormal_F_40 = 0;
        /// <summary>
        /// 异常重其他
        /// </summary>
        private int abnormal_F_other = 0;
        /// <summary>
        /// 异常重合计
        /// </summary>
        private int abnormal_F_total = 0;

        public int Forecast_total
        {
            get
            {
                return Forecast_E_total + Forecast_F_total;
            }

            set
            {
                forecast_total = value;
            }
        }

        public int Forecast_E_20
        {
            get
            {
                return forecast_E_20;
            }

            set
            {
                forecast_E_20 = value;
            }
        }

        public int Forecast_E_40
        {
            get
            {
                return forecast_E_40;
            }

            set
            {
                forecast_E_40 = value;
            }
        }

        public int Forecast_E_other
        {
            get
            {
                return forecast_E_other;
            }

            set
            {
                forecast_E_other = value;
            }
        }

        public int Forecast_E_total
        {
            get
            {
                return forecast_E_20 + forecast_E_40 + forecast_E_other;
            }

            set
            {
                forecast_E_total = value;
            }
        }

        public int Forecast_F_20
        {
            get
            {
                return forecast_F_20;
            }

            set
            {
                forecast_F_20 = value;
            }
        }

        public int Forecast_F_40
        {
            get
            {
                return forecast_F_40;
            }

            set
            {
                forecast_F_40 = value;
            }
        }

        public int Forecast_F_other
        {
            get
            {
                return forecast_F_other;
            }

            set
            {
                forecast_F_other = value;
            }
        }

        public int Forecast_F_total
        {
            get
            {
                return forecast_F_20 + forecast_F_40 + forecast_F_other;
            }

            set
            {
                forecast_F_total = value;
            }
        }

        public int Tally_total
        {
            get
            {
                return Tally_E_total + Tally_F_total;
            }

            set
            {
                tally_total = value;
            }
        }

        public int Tally_E_20
        {
            get
            {
                return tally_E_20;
            }

            set
            {
                tally_E_20 = value;
            }
        }

        public int Tally_E_40
        {
            get
            {
                return tally_E_40;
            }

            set
            {
                tally_E_40 = value;
            }
        }

        public int Tally_E_other
        {
            get
            {
                return tally_E_other;
            }

            set
            {
                tally_E_other = value;
            }
        }

        public int Tally_E_total
        {
            get
            {
                return tally_E_20 + tally_E_40 + tally_E_other;
            }

            set
            {
                tally_E_total = value;
            }
        }

        public int Tally_F_20
        {
            get
            {
                return tally_F_20;
            }

            set
            {
                tally_F_20 = value;
            }
        }

        public int Tally_F_40
        {
            get
            {
                return tally_F_40;
            }

            set
            {
                tally_F_40 = value;
            }
        }

        public int Tally_F_other
        {
            get
            {
                return tally_F_other;
            }

            set
            {
                tally_F_other = value;
            }
        }

        public int Tally_F_total
        {
            get
            {
                return tally_F_20 + tally_F_40 + tally_F_other;
            }

            set
            {
                tally_F_total = value;
            }
        }

        public int Abnormal_total
        {
            get
            {
                return Abnormal_E_total + Abnormal_F_total;
            }

            set
            {
                abnormal_total = value;
            }
        }

        public int Abnormal_E_20
        {
            get
            {
                return abnormal_E_20;
            }

            set
            {
                abnormal_E_20 = value;
            }
        }

        public int Abnormal_E_40
        {
            get
            {
                return abnormal_E_40;
            }

            set
            {
                abnormal_E_40 = value;
            }
        }

        public int Abnormal_E_other
        {
            get
            {
                return abnormal_E_other;
            }

            set
            {
                abnormal_E_other = value;
            }
        }

        public int Abnormal_E_total
        {
            get
            {
                return abnormal_E_20 + abnormal_E_40 + abnormal_E_other;
            }

            set
            {
                abnormal_E_total = value;
            }
        }

        public int Abnormal_F_20
        {
            get
            {
                return abnormal_F_20;
            }

            set
            {
                abnormal_F_20 = value;
            }
        }

        public int Abnormal_F_40
        {
            get
            {
                return abnormal_F_40;
            }

            set
            {
                abnormal_F_40 = value;
            }
        }

        public int Abnormal_F_other
        {
            get
            {
                return abnormal_F_other;
            }

            set
            {
                abnormal_F_other = value;
            }
        }

        public int Abnormal_F_total
        {
            get
            {
                return abnormal_F_20 + abnormal_F_40 + abnormal_F_other;
            }

            set
            {
                abnormal_F_total = value;
            }
        }
    }
}