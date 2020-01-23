namespace Trail365.ViewModels
{
    public abstract class DatapagerViewModel
    {
        public string PageAction { get; set; }

        public string PageController { get; set; }

        public bool EnablePaging { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 3;

        public int SkipEntries => this.EnablePaging ? (this.Page - 1) * this.PageSize : 0;

        public int LastPage => this.PageSize > 0 ? this.UnpagedResults / this.PageSize + 1 : 1;

        public bool IsFirstPage => this.Page == 1;

        public bool IsLastPage => this.Page == this.LastPage;

        public int UnpagedResults { get; set; }
    }
}
