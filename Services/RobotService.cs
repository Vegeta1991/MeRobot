using MeRobot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Ports;

namespace MeRobot.Services
{
    public class RobotService : BackgroundService
    {
        private readonly ILogger<RobotService> _logger;//Wypisuje co zostaje robione teraz w obiekcie

        private readonly RobotState _state = new();//Istancja RobotState
        private readonly object _stateLock = new();//Obiekt state ma kilka komponentow naraz temperatura, bateria.Zeby nie bylo haosu loker pozwala
        //zachowac porzadek pilnuje zeby wykonywala sie jedna rzecz po koleji a nie wszystkie na raz czyli zmiana tem zmiana stanu baterii ecc.
        private readonly SerialPort _serialPort = new SerialPort("/dev/ttyAMA0", 115200)
        {
            ReadTimeout = 1000,//czas oczekiwania na dane z portu, jesli nie przyjdą w tym czasie to rzuci wyjątek
        };
        //Port UART do komunikacji z robotem.
        //teraz tak albo tworze obiekt tu albo w konstruktorze , jezeli mam stale dane Tu jezeli nie to w konstruktorze .

        public RobotService(ILogger<RobotService> logger)// Tworzymy konstruktor zeby miec gotowy obiekt i moc go uzyc w innych miejscach.
        {
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken ct)//ct - cancellation token(stopuje dzialanie)
        {
            _logger.LogInformation("RobotService started");//Wysyłamy informację do logów, że robot wystartował

            return Task.WhenAll(//Uruchamiamy dwie pętle asynchronicznie, ct daje znać kiedy zatrzymać pętle.
                ControlLoop(ct),
                SensorLoop(ct)
            );
        }

        // =========================
        // CONTROL LOOP (movement)
        // =========================
        private async Task ControlLoop(CancellationToken ct)
        {
            _logger.LogInformation("ControlLoop started");

            while (!ct.IsCancellationRequested)// dopuki aplikacja dziala petla sie kreci
            {
                // TODO: movement logic later
                _logger.LogInformation("ControlLoop tick");

                await Task.Delay(20, ct); // ~50 Hz// przerwa i od nowa lut ct czyli zamkniecie odrazu
            }

            _logger.LogInformation("ControlLoop stopped");
        }

        // =========================
        // SENSOR LOOP (telemetry)
        // =========================
        private async Task SensorLoop(CancellationToken ct)
        //!!!!!!!!!
        //Sprawdz nazwe portu w terminalu miniPC, potem w dokumentacji predkosc komunikacyjna portu UARD,
        //potem w kodzie ustaw te same wartosci. Wtedy bedziesz mial realne dane z czujnikow a nie losowe jak teraz.

        {
            byte[] packetBuffer = new byte[26];//bufor do przechowywania danych z portu, pakiet 26 bajtowy(z dokumentacji robota)
            try
            {
                _serialPort.Open();
                _logger.LogInformation("Serial port opened");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to open serial port" + ex.Message);
                return;//Jezeöli port sie nie otworzy to przerywamy
            }
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    int bytesRead = _serialPort.Read(packetBuffer, 0, packetBuffer.Length);//od 0 do 26 bajtow
                    //czytamy dane z portu do bufora
                    if (bytesRead == packetBuffer.Length)//jesli przeczytano 26 bajtow to mamy pelny pakiet
                    {
                        lock (_stateLock)//blokujemy stan robota zeby nikt inny nie zmienial go w tym czasie
                        {
                            _state.BatteryVoltageMv = BitConverter.ToInt16(packetBuffer, 0);//odczytujemy napiecie baterii z pierwszych 2 bajtow
                            _state.BatteryTemperatureC = BitConverter.ToInt16(packetBuffer, 2);//odczytujemy temperature baterii z kolejnych 2 bajtow
                            _state.BumpDetected = packetBuffer[4] != 0;//odczytujemy czy wykryto kolizje z 5 bajta (0 lub 1)
                            _state.LastUpdatedUtc = DateTime.UtcNow;//aktualizujemy czas ostatniej aktualizacji stanu
                        }
                    }
                    _logger.LogDebug(
                        "Sensor updated : Voltage={Voltage}mV, Temp={Temp}C, Bump={Bump}",
                        _state.BatteryVoltageMv,
                        _state.BatteryTemperatureC,
                        _state.BumpDetected
                        );
                }
                catch (TimeoutException)
                {
                    _logger.LogWarning("Serial port read timeout");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error reading from serial port: " + ex.Message);
                }
                await Task.Delay(100, ct); // ~10 Hz
                
            }
            _logger.LogInformation("SensorLoop stopped");
        }





        public RobotState GetState()
        {
            lock (_stateLock)
            {
                return new RobotState
                {
                    BatteryVoltageMv = _state.BatteryVoltageMv,
                    BatteryTemperatureC = _state.BatteryTemperatureC,
                    BumpDetected = _state.BumpDetected,
                    LastUpdatedUtc = _state.LastUpdatedUtc
                };
            }
        }
    }

}
