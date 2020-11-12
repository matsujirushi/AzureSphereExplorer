using AzureSpherePublicAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Threading; // CancellationTokenSource
using System.Runtime.CompilerServices;
using System.Net.Http;

namespace AzureSphereExplorer
{
    class ModelManager
    {
        internal AzureSphereAPI Api = new AzureSphereAPI();
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        // member
        private List<TenantModel> TenantModels;
        private List<ProductModel> ProductModels;
        private List<DeviceGroupModel> DeviceGroupModels;
        private List<DeviceModel> DeviceModels;
        private List<DeviceInsightModel> DeviceInsightModels;
        private List<UserModel> UsersModels;
        public event EventHandler<EventArgs> NotificationChangeProduct;
        public event EventHandler<EventArgs> NotificationChangeDeviceGroup;

        private static ModelManager Instatnce = new ModelManager();

        private List<TenantModel> ParseTenantModel(List<AzureSphereTenant> tenants)
        {
            List<TenantModel> tenantModels = new List<TenantModel>();
            foreach (AzureSphereTenant tenant in tenants)
            {
                TenantModel model = new TenantModel();
                model.Context = tenant;
                model.Tenant = tenant.Name;
                tenantModels.Add(model);
            }
            return tenantModels;
        }

        private List<ProductModel> ParseProductModel(List<AzureSphereProduct> products)
        {
            List<ProductModel> productModels = new List<ProductModel>();
            foreach (AzureSphereProduct product in products)
            {
                ProductModel model = new ProductModel();
                model.Context = product;
                model.Product = product.Name;
                model.Description = product.Description;
                productModels.Add(model);
            }
            return productModels;
        }

        private string GetProductNameForId(string id)
        {
            foreach (ProductModel product in this.ProductModels)
            {
                if (id == product.Context.Id)
                {
                    return product.Product;
                }
            }
            return null;
        }

        private DeviceGroupModel GetDeviceGroupModel(string id)
        {
            foreach (DeviceGroupModel group in this.DeviceGroupModels)
            {
                if (id == group.Context.Id)
                {
                    return group;
                }
            }
            return null;
        }

        private List<DeviceGroupModel> ParseDeviceGroupModel(List<AzureSphereDeviceGroup> groups)
        {
            List<DeviceGroupModel> groupsModels = new List<DeviceGroupModel>();

            foreach (AzureSphereDeviceGroup group in groups)
            {
                DeviceGroupModel model = new DeviceGroupModel();
                model.Context = group;
                model.DeviceGroup = group.Name;
                model.Description = group.Description;
                model.Product = GetProductNameForId(group.ProductId);
                model.OsFeedType = group.OsFeedTypeStr;
                model.UpdatePolicy = group.UpdatePolicyStr;
                model.CurrentDeploymentDate = group.CurrentDeployment?.DeploymentDateUtc.ToLocalTime();
                groupsModels.Add(model);
            }
            return groupsModels;
        }

        private List<DeviceModel> ParseDeviceModel(List<AzureSphereDevice> devices)
        {
            List<DeviceModel> devicesModels = new List<DeviceModel>();

            foreach (AzureSphereDevice device in devices)
            {
                DeviceModel model = new DeviceModel();
                DeviceGroupModel group = GetDeviceGroupModel(device.DeviceGroupId);

                model.Context = device;
                model.ChipSku = device.ChipSkuStr;
                model.Id = device.Id;
                if (group != null)
                {
                    model.DeviceGroup = group.DeviceGroup;
                    model.Product = group.Product;
                }
                devicesModels.Add(model);
            }
            return devicesModels;
        }

        private List<DeploymentModel> ParseDeploymentModel(List<AzureSphereDeployment> deployments)
        {
            List<DeploymentModel> deploymentsModels = new List<DeploymentModel>();

            foreach (AzureSphereDeployment deployment in deployments)
            {
                DeploymentModel model = new DeploymentModel();

                model.Context = deployment;
                model.CurrentDeploymentDate = deployment.DeploymentDateUtc.ToLocalTime();
                model.NumberOfImages = deployment.DeployedImages.Count;
                deploymentsModels.Add(model);

            }
            return deploymentsModels;
        }

        private List<DeviceInsightModel> ParseDeviceInsightModel(List<AzureSphereDeviceInsight> insights)
        {
            List<DeviceInsightModel> deviceInsightModelModels = new List<DeviceInsightModel>();

            foreach (AzureSphereDeviceInsight insight in insights)
            {
                DeviceInsightModel model = new DeviceInsightModel();

                model.Context = insight;
                model.DeviceId = insight.DeviceId;
                model.StartTime = insight.StartTimestamp.ToLocalTime();
                model.EndTime = insight.EndTimestamp.ToLocalTime();
                model.Description = insight.Description;
                model.EventType = insight.EventType;
                model.EventClass = insight.EventClass;
                model.EventCategory = insight.EventCategory;
                model.EventCount = insight.EventCount;
                deviceInsightModelModels.Add(model);

            }
            return deviceInsightModelModels;
        }

        private List<UserModel> ParseUsersModel(List<AzureSphereUser> users)
        {
            List<UserModel> usersModels = new List<UserModel>();

            foreach (AzureSphereUser user in users)
            {
                UserModel model = new UserModel();

                model.Context = user;
                model.User = user.DisplayName;
                model.Mail = user.Mail;
                model.Roles = string.Join(",", user.Roles);
                usersModels.Add(model);

            }
            return usersModels;
        }

        private ImageModel ParseImageModel(AzureSphereImage image)
        {
            ImageModel model = new ImageModel();
            model.Context = image;
            model.Image = image.Name;
            model.Description = image.Description;
            return model;
        }

        private async Task<List<TenantModel>> GetTenantsAsync()
        {
            List<AzureSphereTenant> tenants = await Api.GetTenantsAsync(cancellationTokenSource.Token);
            return ParseTenantModel(tenants);
        }

        private async Task<List<ProductModel>> GetProductsAsync(AzureSphereTenant tenant)
        {
            List<AzureSphereProduct> products = await Api.GetProductsAsync(tenant, cancellationTokenSource.Token);
            return  ParseProductModel(products);
        }

        private async Task<bool> DeleteProductAsync(AzureSphereTenant tenant, AzureSphereProduct product)
        {
            return  await Api.DeleteProductAsync(tenant, product, cancellationTokenSource.Token);
        }

        private async Task<List<DeviceGroupModel>> GetDeviceGroupsAsync(AzureSphereTenant tenant)
        {
            List<AzureSphereDeviceGroup> groups = await Api.GetDeviceGroupsAsync(tenant, cancellationTokenSource.Token);
            return ParseDeviceGroupModel(groups);
        }

        private async Task<bool> DeleteDeviceGroupAsync(AzureSphereTenant tenant, AzureSphereDeviceGroup group)
        {
            return await Api.DeleteDeviceGroupAsync(tenant, group, cancellationTokenSource.Token);
        }

        private async Task<List<DeviceModel>> GetDevicesAsync(AzureSphereTenant tenant)
        {
            List<AzureSphereDevice> devices = await Api.GetDevicesAsync(tenant, cancellationTokenSource.Token);
            return ParseDeviceModel(devices);
        }

        private async Task<List<DeploymentModel>> GetDeploymentsAsync(AzureSphereTenant tenant, AzureSphereDeviceGroup group)
        {
            List<AzureSphereDeployment> deployments = await Api.GetDeploymentsAsync(tenant, group, cancellationTokenSource.Token);
            return ParseDeploymentModel(deployments);
        }

        private async Task<List<DeviceInsightModel>> GetDeviceInsightsAsync(AzureSphereTenant tenant)
        {
            List<AzureSphereDeviceInsight> insights = await Api.GetDeviceInsightsAsync(tenant, cancellationTokenSource.Token);
            return this.DeviceInsightModels = ParseDeviceInsightModel(insights);
        }
        public async Task<List<string>> GetRolesAsync(AzureSphereTenant tenant, string username)
        {
            List<string> roles = await Api.GetRolesAsync(tenant, username, cancellationTokenSource.Token);
            return roles;
        }
        private async Task<List<UserModel>> GetUsersAsync(AzureSphereTenant tenant)
        {
            List<AzureSphereUser> users = await Api.GetUsersAsync(tenant, cancellationTokenSource.Token);
            return this.UsersModels = ParseUsersModel(users);
        }

        private async Task<List<ImageModel>> GetImagesAsync(AzureSphereTenant tenant, AzureSphereDeployment deployment)
        {
            List<ImageModel> imageModels = new List<ImageModel>();

            try
            {
                foreach (string imageId in deployment.DeployedImages) {
                    AzureSphereImage image = await Api.GetImageAsync(tenant, imageId, cancellationTokenSource.Token);
                    imageModels.Add(ParseImageModel(image));
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return null;
            }
            return imageModels;
        }

        public static ModelManager GetInstance()
        {
            return Instatnce;
        }

        public async Task<bool> Initialize()
        {
            // 認証成功するか
            try
            {
                await Api.AuthenticationAsync(cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                // 認証失敗
                Console.Error.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }


        // tenant 一覧を返す
        public async Task<List<TenantModel>> GetTenantModels()
        {
            try
            {
                if (this.TenantModels == null)
                {
                    this.TenantModels = await GetTenantsAsync();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return null;
            }
            return this.TenantModels;
        }

        // product 一覧を返す
        public async Task<List<ProductModel>> GetProductModels(TenantModel tenantModel, bool force)
        {
            try
            {
                if (this.ProductModels == null || force)
                {
                    this.ProductModels = await GetProductsAsync(tenantModel.Context);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return null;
            }
            return this.ProductModels;
        }
        public string GetUsername()
        {
            return Api.Username;
        }

        public async Task<bool> DeleteProduct(TenantModel tenantModel, ProductModel productModel)
        {
            bool ret = false;
            try
            {
                if (await DeleteProductAsync(tenantModel.Context, productModel.Context))
                {
                    ret = true;
                    this.ProductModels = await GetProductsAsync(tenantModel.Context);
                    this.DeviceGroupModels = await GetDeviceGroupsAsync(tenantModel.Context);

                    NotificationChangeProduct?.Invoke(this, null);
                    NotificationChangeDeviceGroup?.Invoke(this, null);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return false;
            }
            return ret;
        }

        // DeviceGroup list
        public async Task<List<DeviceGroupModel>> GetDeviceGroupModels(TenantModel tenantModel, bool force)
        {
            try
            {
                if (this.DeviceGroupModels == null || force)
                {
                    this.DeviceGroupModels = await GetDeviceGroupsAsync(tenantModel.Context);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return null;
            }
            return this.DeviceGroupModels;
        }

        public async Task<bool> DeleteDeviceGroup(TenantModel tenantModel, DeviceGroupModel deviceGroupModel)
        {
            bool ret = false;
            try
            {
                if (await DeleteDeviceGroupAsync(tenantModel.Context, deviceGroupModel.Context))
                {
                    ret = true;
                    this.DeviceGroupModels = await GetDeviceGroupsAsync(tenantModel.Context);
                    NotificationChangeDeviceGroup?.Invoke(this, null);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return false;
            }
            return ret;
        }

        // Device list
        public async Task<List<DeviceModel>> GetDeviceModels(TenantModel tenantModel, bool force)
        {
            // DeviceGroupModels is already getting
            try
            {
                if (this.DeviceModels == null || force)
                {
                    this.DeviceModels = await GetDevicesAsync(tenantModel.Context);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return null;
            }
            return this.DeviceModels;
        }

        // Deployment list
        public async Task<List<DeploymentModel>> GetDeploymentModels(TenantModel tenantModel, DeviceGroupModel deviceGroupModel)
        {
            List<DeploymentModel> deploymentModels;

            if(deviceGroupModel == null)
            {
                return null;
            }

            try
            {
                deploymentModels = await GetDeploymentsAsync(tenantModel.Context, deviceGroupModel.Context);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return null;
            }
            return deploymentModels;

        }

        // DeviceInsight list
        public async Task<List<DeviceInsightModel>> GetDeviceInsightModels(TenantModel tenantModel)
        {
            try
            {
                if (this.DeviceInsightModels == null)
                {
                    this.DeviceInsightModels = await GetDeviceInsightsAsync(tenantModel.Context);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return null;
            }
            return this.DeviceInsightModels;

        }

        // Users list
        public async Task<List<UserModel>> GetUsersModels(TenantModel tenantModel)
        {
            try
            {
                if (this.UsersModels == null)
                {
                    this.UsersModels = await GetUsersAsync(tenantModel.Context);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return null;
            }
            return this.UsersModels;

        }


        // Image list
        public async Task<List<ImageModel>> GetImageModels(TenantModel tenantModel, DeploymentModel deploymentModel)
        {
            List<ImageModel> imageModels;
            try
            {
                imageModels = await GetImagesAsync(tenantModel.Context, deploymentModel.Context);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return null;
            }
            return imageModels;

        }
    }
}
