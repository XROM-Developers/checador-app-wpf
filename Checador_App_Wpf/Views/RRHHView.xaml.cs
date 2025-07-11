using Checador_App_Wpf.Models;
using Checador_App_Wpf.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using Checador_App_Wpf.Controllers;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using Checador_App_Wpf.Components.Fingerprints;

namespace ControlDeCheckeo.Views
{
    public partial class RRHHView : UserControl
    {
        private readonly EmpleadoService _empleadoService;
        private List<Empleado> todosLosEmpleados = new();
        FingerprintRegisterControl fingerprintRegisterControl;


        public RRHHView()
        {
            InitializeComponent();
            _empleadoService = new EmpleadoService();
            CargarEmpleados();
            MostrarUsuarioActual();
        }

        private async void CargarEmpleados()
        {
            Debug.WriteLine("🔄 Cargando lista de empleados...");
            MainWindow.Instance.MostrarLoader("Cargando empleados...");

            todosLosEmpleados = await _empleadoService.GetEmpleadosAsync();

            MainWindow.Instance.OcultarLoader();

            if (todosLosEmpleados != null)
            {
                Debug.WriteLine(todosLosEmpleados);
                Debug.WriteLine($"✅ {todosLosEmpleados.Count} empleados cargados.");
                lstEmpleados.ItemsSource = todosLosEmpleados;
            }
            else
            {
                Debug.WriteLine("❌ No se pudo cargar la lista de empleados.");
                lstEmpleados.Items.Add("No se pudo cargar la lista de empleados.");
            }
        }

        private void txtBuscarEmpleado_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = QuitarAcentos(txtBuscarEmpleado.Text.Trim().ToLower());
            Debug.WriteLine($"🔍 Buscando empleados con filtro: '{filtro}'");

            var filtrados = todosLosEmpleados
                .Where(emp => QuitarAcentos(emp.nombreUsuario.ToLower()).Contains(filtro))
                .ToList();

            Debug.WriteLine($"🔎 Resultados encontrados: {filtrados.Count}");
            lstEmpleados.ItemsSource = filtrados;
        }

        private string QuitarAcentos(string texto)
        {
            var normalized = texto.Normalize(NormalizationForm.FormD);
            return new string(normalized
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());
        }

        private async void lstEmpleados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstEmpleados.SelectedItem is Empleado empleadoSeleccionado)
            {
                Debug.WriteLine($"👤 Empleado seleccionado: {empleadoSeleccionado.nombreUsuario} (ID: {empleadoSeleccionado.idUsuario})");

                MainWindow.Instance.MostrarLoader("Obteniendo detalles del empleado...");

                var detalleEmpleado = await _empleadoService.GetEmpleadoAsync(empleadoSeleccionado.idUsuario);

                MainWindow.Instance.OcultarLoader();

                if (detalleEmpleado != null)
                {
                    Debug.WriteLine($"📋 Detalles del empleado obtenidos correctamente para ID {detalleEmpleado.IdUsuario}.");
                    MostrarDetalleEmpleado(detalleEmpleado);

                    if (registerControl.IsInitialized)
                    {
                        Debug.WriteLine("✅ Lector inicializado. Preparando huellas para nuevo usuario...");

                        // ✅ Cargar huellas y resetear lector
                        await registerControl.PrepararRegistroParaUsuario(detalleEmpleado.IdUsuario);
                    }
                    else
                    {
                        Debug.WriteLine("⚠️ Lector no inicializado.");
                        MessageBox.Show("El lector aún no está listo. Espere un momento e intente de nuevo.", "Lector no iniciado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    Debug.WriteLine($"❌ No se pudieron obtener los detalles del empleado con ID {empleadoSeleccionado.idUsuario}");
                }
            }
        }


        private async void MostrarDetalleEmpleado(EmpleadoDetalle empleado)
        {
            Debug.WriteLine($"🧾 Mostrando detalles para: {empleado.NombreUsuario}");

            Nombre.Text = (empleado.NombreUsuario + " " + empleado.ApellidoPaternoUsuario + " " + empleado.ApellidoMaternoUsuario) ?? "Sin nombre";
            RFC.Text = empleado.ClaveUsuario ?? "Sin RFC";
            NumeroEmpleado.Text = empleado.ClaveUsuario ?? "Sin número";
            Departamento.Text = empleado.NombreArea ?? "Sin departamento";
            Cargo.Text = empleado.NombreRol ?? "Sin puesto";

            try
            {
                if (!string.IsNullOrWhiteSpace(empleado.FotoUsuarioURL))
                {
                    userPhoto.Source = new BitmapImage(new Uri(empleado.FotoUsuarioURL));
                }
                else
                {
                    userPhoto.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/Avatar.png"));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Error al cargar la imagen: {ex.Message}");
                userPhoto.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/Avatar.png"));
            }

            if (fingerprintRegisterControl != null && empleado.IdUsuario > 0)
            {
                try
                {
                    await fingerprintRegisterControl.PrepararRegistroParaUsuario(empleado.IdUsuario);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Error al recargar huellas del usuario: {ex.Message}");
                }
            }
        }


        private async void MostrarUsuarioActual()
        {
            var user = AuthController.UsuarioActual;

            if (user != null)
            {
                Debug.WriteLine($"👤 Usuario que atiende: {user.NombreUsuario} | Clave: {user.ClaveUsuario} | Puesto: {user.PuestoUsuario}");

                NombreRH.Text = user.NombreUsuario ?? "Sin nombre";
                NumeroEmpleadoRH.Text = user.ClaveUsuario ?? "Sin número";
                CargoRH.Text = user.PuestoUsuario ?? "Sin puesto";

                try
                {
                    var detalleEmpleado = await _empleadoService.GetEmpleadoAsync(user.IdUsuario);
                    if (detalleEmpleado != null)
                    {
                        if (!string.IsNullOrWhiteSpace(detalleEmpleado.FotoUsuarioURL))
                        {
                            actualUserPhoto.Source = new BitmapImage(new Uri(detalleEmpleado.FotoUsuarioURL));
                        }
                        else
                        {
                            actualUserPhoto.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/Avatar.png"));
                        }
                    }
                    else
                    {
                        Debug.WriteLine("⚠️ No se pudieron obtener los detalles del usuario actual.");
                        actualUserPhoto.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/Avatar.png"));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Error al cargar detalles del usuario actual: {ex.Message}");
                    actualUserPhoto.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/Avatar.png"));
                }
            }
            else
            {
                Debug.WriteLine("⚠️ No hay usuario autenticado disponible.");
            }
        }

    }
}
