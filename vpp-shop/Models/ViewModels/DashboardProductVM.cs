namespace vpp_shop.Models.ViewModels
{
    public class DashboardProductVM
    {
        public List<ProductRankingVM> Top3 { get; set; }
        public List<ProductRankingVM> Top4To10 { get; set; }
        public List<string> LowOrNoSales { get; set; }

        public int BanChay { get; set; }
        public int BanIt { get; set; }
        public int KhongBan { get; set; }
    }

}
