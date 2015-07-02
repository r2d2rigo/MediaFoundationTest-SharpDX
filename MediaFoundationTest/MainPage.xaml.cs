using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MediaFoundationTest.Resources;
using SharpDX.SimpleInitializer;
using SharpDX.MediaFoundation;
using Windows.ApplicationModel;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX;
using System.Runtime.InteropServices;
using SharpDX.IO;
using System.Threading;
using Windows.Devices.Sensors;
using System.Diagnostics;

namespace MediaFoundationTest
{
    public partial class MainPage : PhoneApplicationPage
    {
        float rotX;
        float rotY;

        private SharpDXContext context;
        private MediaEngine mediaEngine;
        // private MediaEngineEx mediaEngineEx;
        private MediaEngineClassFactory mediaFactory;
        private Texture2D texture;
        private Surface dxgiSurface;
        private DXGIDeviceManager deviceManager;
        private SharpDX.Direct3D11.Buffer vertexBuffer;
        private SharpDX.Direct3D11.Buffer indexBuffer;
        private SharpDX.Direct3D11.Buffer constantBuffer;
        private VertexBufferBinding bufferBinding;
        private VertexShader vertexShader;
        private PixelShader pixelShader;
        private InputLayout inputLayout;
        private SamplerState sampler;
        private ShaderResourceView textureView;

        // Constructor
        public MainPage()
        {
            MediaManager.Startup();

            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //this.mediaEngine.Play();
            //this.mediaEngine.Load();
            // this.mediaEngine.GetNativeVideoSize(out width, out height);

            context = new SharpDXContext();
            context.Render += context_Render;
            context.DeviceReset += context_DeviceReset;
            context.BindToControl(this.Surface);

            DeviceMultithread mt = context.D3DDevice.QueryInterface<DeviceMultithread>();
            mt.SetMultithreadProtected(true);

            deviceManager = new DXGIDeviceManager();
            deviceManager.ResetDevice(context.D3DDevice);

            MediaEngineAttributes attr = new MediaEngineAttributes();
            attr.VideoOutputFormat = (int)SharpDX.DXGI.Format.B8G8R8A8_UNorm;
            attr.DxgiManager = deviceManager;

            this.mediaFactory = new MediaEngineClassFactory();
            this.mediaEngine = new MediaEngine(this.mediaFactory, attr, MediaEngineCreateFlags.None);
            // this.mediaEngineEx = this.mediaEngine.QueryInterface<MediaEngineEx>();
            this.mediaEngine.PlaybackEvent += mediaEngine_PlaybackEvent;

            this.mediaEngine.Source = Package.Current.InstalledLocation.Path + "/Assets/keloid.mp4";
            this.mediaEngine.Load();


            ushort[] indices = new ushort[]
            {
                0, 1, 2,
                2, 3, 0,
            };

            this.indexBuffer = SharpDX.Direct3D11.Buffer.Create<ushort>(this.context.D3DDevice, BindFlags.IndexBuffer, indices);

            float[] vertices = new float[]
            {
                                      // 3D coordinates              UV Texture coordinates
                                      -1.0f, -1.0f, -1.0f, 1.0f,     0.0f, 1.0f, // Front
                                      -1.0f,  1.0f, -1.0f, 1.0f,     0.0f, 0.0f,
                                       1.0f,  1.0f, -1.0f, 1.0f,     1.0f, 0.0f,
                                      -1.0f, -1.0f, -1.0f, 1.0f,     0.0f, 1.0f,
                                       1.0f,  1.0f, -1.0f, 1.0f,     1.0f, 0.0f,
                                       1.0f, -1.0f, -1.0f, 1.0f,     1.0f, 1.0f,

                                      -1.0f, -1.0f,  1.0f, 1.0f,     1.0f, 0.0f, // BACK
                                       1.0f,  1.0f,  1.0f, 1.0f,     0.0f, 1.0f,
                                      -1.0f,  1.0f,  1.0f, 1.0f,     1.0f, 1.0f,
                                      -1.0f, -1.0f,  1.0f, 1.0f,     1.0f, 0.0f,
                                       1.0f, -1.0f,  1.0f, 1.0f,     0.0f, 0.0f,
                                       1.0f,  1.0f,  1.0f, 1.0f,     0.0f, 1.0f,

                                      -1.0f, 1.0f, -1.0f,  1.0f,     0.0f, 1.0f, // Top
                                      -1.0f, 1.0f,  1.0f,  1.0f,     0.0f, 0.0f,
                                       1.0f, 1.0f,  1.0f,  1.0f,     1.0f, 0.0f,
                                      -1.0f, 1.0f, -1.0f,  1.0f,     0.0f, 1.0f,
                                       1.0f, 1.0f,  1.0f,  1.0f,     1.0f, 0.0f,
                                       1.0f, 1.0f, -1.0f,  1.0f,     1.0f, 1.0f,

                                      -1.0f,-1.0f, -1.0f,  1.0f,     1.0f, 0.0f, // Bottom
                                       1.0f,-1.0f,  1.0f,  1.0f,     0.0f, 1.0f,
                                      -1.0f,-1.0f,  1.0f,  1.0f,     1.0f, 1.0f,
                                      -1.0f,-1.0f, -1.0f,  1.0f,     1.0f, 0.0f,
                                       1.0f,-1.0f, -1.0f,  1.0f,     0.0f, 0.0f,
                                       1.0f,-1.0f,  1.0f,  1.0f,     0.0f, 1.0f,

                                      -1.0f, -1.0f, -1.0f, 1.0f,     0.0f, 1.0f, // Left
                                      -1.0f, -1.0f,  1.0f, 1.0f,     0.0f, 0.0f,
                                      -1.0f,  1.0f,  1.0f, 1.0f,     1.0f, 0.0f,
                                      -1.0f, -1.0f, -1.0f, 1.0f,     0.0f, 1.0f,
                                      -1.0f,  1.0f,  1.0f, 1.0f,     1.0f, 0.0f,
                                      -1.0f,  1.0f, -1.0f, 1.0f,     1.0f, 1.0f,

                                       1.0f, -1.0f, -1.0f, 1.0f,     1.0f, 0.0f, // Right
                                       1.0f,  1.0f,  1.0f, 1.0f,     0.0f, 1.0f,
                                       1.0f, -1.0f,  1.0f, 1.0f,     1.0f, 1.0f,
                                       1.0f, -1.0f, -1.0f, 1.0f,     1.0f, 0.0f,
                                       1.0f,  1.0f, -1.0f, 1.0f,     0.0f, 0.0f,
                                       1.0f,  1.0f,  1.0f, 1.0f,     0.0f, 1.0f,            };

            this.vertexBuffer = SharpDX.Direct3D11.Buffer.Create<float>(this.context.D3DDevice, BindFlags.VertexBuffer, vertices);
            this.bufferBinding = new VertexBufferBinding(this.vertexBuffer, sizeof(float) * 6, 0);

            var path = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;

            // Loads vertex shader bytecode
            var vertexShaderByteCode = NativeFile.ReadAllBytes(path + "\\MiniCubeTexture_VS.fxo");
            this.vertexShader = new VertexShader(this.context.D3DDevice, vertexShaderByteCode);

            this.inputLayout = new InputLayout(this.context.D3DDevice, vertexShaderByteCode, new[]
                    {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 16, 0)
                    });

            this.pixelShader = new PixelShader(this.context.D3DDevice, NativeFile.ReadAllBytes(path + "\\MiniCubeTexture_PS.fxo"));

            this.constantBuffer = new SharpDX.Direct3D11.Buffer(this.context.D3DDevice, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            this.sampler = new SamplerState(this.context.D3DDevice, new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                BorderColor = Color.Black,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MipLodBias = 0,
                MinimumLod = -float.MaxValue,
                MaximumLod = float.MaxValue
            });


            base.OnNavigatedTo(e);
        }

        void context_DeviceReset(object sender, DeviceResetEventArgs e)
        {
        }

        private RasterizerState state;

        bool ready = false;

        ManualResetEvent resetEvent = new ManualResetEvent(false);

        void mediaEngine_PlaybackEvent(MediaEngineEvent mediaEvent, long param1, int param2)
        {
            if (mediaEvent == MediaEngineEvent.CanPlay)
            {

                if (this.mediaEngine.Error != null)
                {
                    int a = 0;
                }

                if (this.mediaEngine.HasVideo())
                {
                    int width = 0;
                    int height = 0;
                    this.mediaEngine.GetNativeVideoSize(out width, out height);

                    this.texture = new SharpDX.Direct3D11.Texture2D(this.context.D3DDevice, new SharpDX.Direct3D11.Texture2DDescription()
                        {
                            ArraySize = 1,
                            Width = width,
                            Height = height,
                            Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                            Format = Format.B8G8R8A8_UNorm,
                            CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                            BindFlags = SharpDX.Direct3D11.BindFlags.RenderTarget | SharpDX.Direct3D11.BindFlags.ShaderResource,
                            OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                            SampleDescription = new SampleDescription(1, 0),
                            MipLevels = 1,
                        });

                    this.dxgiSurface = this.texture.QueryInterface<SharpDX.DXGI.Surface>();

                    this.textureView = new ShaderResourceView(this.context.D3DDevice, this.texture);

                    ready = true;

                    this.mediaEngine.Play();
                }

                state = new RasterizerState(this.context.D3DDevice, new RasterizerStateDescription()
                    {
                        CullMode = CullMode.Back,
                        FillMode = FillMode.Solid,
                        DepthBias = 0,
                        DepthBiasClamp = 0,
                        IsDepthClipEnabled = true,
                        IsMultisampleEnabled = true,
                        SlopeScaledDepthBias = 0
                    });
            }
        }

        float angle = 0.0f;

        void context_Render(object sender, EventArgs e)
        {
            if (!ready)
                return;

            if (this.dxgiSurface == null)
                return;

            long ts;
            if (!mediaEngine.OnVideoStreamTick(out ts))
            {
                return;
            }

            if (ts < 0)
                return;

            this.mediaEngine.TransferVideoFrame(this.dxgiSurface, null, new SharpDX.Rectangle(0, 0, this.texture.Description.Width, this.texture.Description.Height), null);

            rotX += 0.01f;
            rotY += 0.02f;

            Matrix rot = Matrix.RotationYawPitchRoll(rotY, rotX, 0);

            var view = SharpDX.Matrix.LookAtLH(new Vector3(0, 0, -5), Vector3.Zero, Vector3.UnitY);
            var proj = SharpDX.Matrix.PerspectiveFovLH((float)(Math.PI / 4.0f), surfaceWidth / surfaceHeight, 0.5f, 1000);
            var viewProj = view * proj;
            context.D3DContext.OutputMerger.SetRenderTargets(context.BackBufferView);
            context.D3DContext.ClearRenderTargetView(context.BackBufferView, SharpDX.Color.CornflowerBlue);

            var worldViewProj = rot * viewProj;
            worldViewProj.Transpose();
            this.context.D3DContext.UpdateSubresource(ref worldViewProj, constantBuffer, 0);

            this.context.D3DContext.Rasterizer.State = state;
            this.context.D3DContext.InputAssembler.SetVertexBuffers(0, this.bufferBinding);
            this.context.D3DContext.InputAssembler.InputLayout = this.inputLayout;
            this.context.D3DContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this.context.D3DContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R16_UNorm, 0);
            this.context.D3DContext.VertexShader.Set(this.vertexShader);
            this.context.D3DContext.VertexShader.SetConstantBuffer(0, this.constantBuffer);
            this.context.D3DContext.PixelShader.SetShaderResource(0, textureView);
            this.context.D3DContext.PixelShader.SetSampler(0, sampler);
            this.context.D3DContext.PixelShader.Set(this.pixelShader);

            this.context.D3DContext.Draw(36, 0);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        float surfaceWidth = 1, surfaceHeight = 1;

        private void Surface_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            surfaceWidth = (float)e.NewSize.Width * 1.6f;
            surfaceHeight = (float)e.NewSize.Height * 1.6f;

            if (surfaceWidth < 1)
                surfaceWidth = 1;
            if (surfaceHeight < 1)
                surfaceHeight = 1;

        }

    }
}