using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text.Json;


namespace TestMQTT
{
     public partial class Form1 : Form
    {        
        private string mqttServer = "broker.hivemq.com";
        private string clientID = "DTI_CANSAT_GROUND";
        private MqttClient mqttClient;

        public Form1()
        {
            InitializeComponent();            
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            MqttClient_PublishMessage();
        }

        void MqttClient_Initialize()
        {
            mqttClient = new MqttClient(mqttServer);            
            mqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;
            
            mqttClient.Connect(clientID);
        }

        void MqttClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Message);
            try
            {
                WeatherForecast weatherForecast =
                    JsonSerializer.Deserialize<WeatherForecast>(message);

                //Debug.WriteLine($"Received: {e.Topic}, {message}");
                Debug.WriteLine(weatherForecast.Date);
                Debug.WriteLine(weatherForecast.TemperatureCelsius);
                Debug.WriteLine(weatherForecast.Summary);
            }
            catch (Exception ex)
            { Debug.WriteLine(ex.Message); }
        }

        private void MqttClient_PublishMessage()
        {
            var weatherForecast = new WeatherForecast
            {
                Date = DateTime.Parse("2019-08-01"),
                TemperatureCelsius = 25,
                Summary = "Hot"
            };
            string jsonString = JsonSerializer.Serialize(weatherForecast);

            if (!mqttClient.IsConnected)
                mqttClient.Connect(clientID);

            if (mqttClient.IsConnected)
                // publish a message on "/home/temperature" topic with QoS 2
                mqttClient.Publish("mymqtt/testmessage", Encoding.UTF8.GetBytes(jsonString), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            else
                Debug.WriteLine("Can't connect to server");
        }

        private void MqttClient_SubscribeTopic()
        {            
            string[] topic = { "testtopic/1", "sensor/humidity" };

            byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE };
            mqttClient.Subscribe(topic, qosLevels);

            Debug.WriteLine("Subsribed to topic");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MqttClient_Initialize();
            MqttClient_SubscribeTopic();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            mqttClient.Disconnect();
        }
    }

    public class WeatherForecast
    {
        public DateTimeOffset Date { get; set; }
        public int TemperatureCelsius { get; set; }
        public string Summary { get; set; }
    }
}