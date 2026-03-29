namespace CarStoreManager.Domain.Enums;

[Flags]
public enum AcessoriosVeiculo
{
    Nenhum             = 0,
    ArCondicionado     = 1 << 0,  // 1
    VidrosEletricos    = 1 << 1,  // 2
    DirecaoHidraulica  = 1 << 2,  // 4
    TetoSolar          = 1 << 3,  // 8
    BancoCouro         = 1 << 4,  // 16
    CameraRe           = 1 << 5,  // 32
    SensorEstacionamento = 1 << 6, // 64
    CentralMultimidia  = 1 << 7,  // 128
    Alarme             = 1 << 8,  // 256
    TravasEletrica     = 1 << 9,  // 512
    RodaLiga           = 1 << 10, // 1024
    Bluetooth          = 1 << 11  // 2048
}