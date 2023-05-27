using CSR.Home;


namespace CSR;

class Program
{


    private static SessionManager? _sessionManager;

    static void Close()
    {
        _sessionManager?.Close();
    }

    static void Main(string[] args)
    {
        Console.Title = "CosmicSaikoRacing - GameServer";
  
        ConfigManager.Load();
        Logger.Load(ConfigManager.Config.GameLogPath);
        CardManager.Load(ConfigManager.Config.CardDataPath);

        _sessionManager = new SessionManager();
        
        //게임서버 시작
        _sessionManager.Run<HomeSession>();

        //콘솔창 관리    
        Console.WriteLine("Type quit to close server");
        while (true)
        {
            string? isQuit = Console.ReadLine()?.ToLower();
            if (isQuit == "quit")
                break;
            //서버 재시작
            if (isQuit == "help")
                Console.WriteLine("Type quit to close server");
        }

        Close();
    }
    //
    // //UPnP로 (internal port:external port)=16969:16969적용
    // private static async void DeviceFound(object? sender, DeviceEventArgs args)
    // {
    //     INatDevice device = args.Device;
    //
    //     var mapping = new Mapping(Protocol.Tcp, config.port, config.port);
    //     mapping = await device.CreatePortMapAsync(mapping);
    //     Logger.Log ($"Create Mapping: protocol={mapping.Protocol}, public={mapping.PublicPort}, private={mapping.PrivatePort}");
    //     NatUtility.StopDiscovery();
    // }
}