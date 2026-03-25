using Royal_Blueberry_Dictionary.Database;
using Royal_Blueberry_Dictionary.Service.ApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Royal_Blueberry_Dictionary.Model;
namespace Royal_Blueberry_Dictionary.Service
{
    public class PackageService
    {
        IBackendApiClient backendApiClient; 
        AppDbContext appDbContext;

        public PackageService(IBackendApiClient backendApiClient, AppDbContext appDbContext)
        {
            this.backendApiClient = backendApiClient;
            this.appDbContext = appDbContext;
        }

        public async Task<List<Package>> getAllPackages()
        {
            var packages = await backendApiClient.GetAsync<List<Package>>(@"api/packages"); 
            return packages;
        }
        public async Task<PackageDetail> getDetailByPackageId(string id) 
        {
            var detail = await backendApiClient.GetAsync<PackageDetail>($@"api/packages/details/{id}");
            return detail; 
        }
    }
}
