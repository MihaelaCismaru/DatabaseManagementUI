using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DatabaseManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OracleConnection connection = new OracleConnection();
        public MainWindow()
        {
            this.setConnection();
            InitializeComponent();
        }

        private void setConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["oracleConnectionString"].ConnectionString;
    
            connection = new OracleConnection(connectionString);
            try
            {
                connection.Open();
            }
            catch (Exception exception) {
                throw exception;
            }
        }
        void updateDataGrid ()
        {
            updatePacientTable();
            updateDiagnosticTable();
            updateTratamentTable();
            updateRetetaTable();
            updateTRTable();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.updateDataGrid();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            connection.Close();
        }

        #region Pacient
        private void updatePacientTable()
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT CNP,NUME,PRENUME,VARSTA,TIPASIGURARE FROM PACIENT ORDER BY NUME DESC";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            PacientTable.ItemsSource = dt.DefaultView;
            reader.Close();
            updateRetetaTable();
        }
        private bool Check_Pacient_Used_In_Reteta(string cnp)
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Add("CNP", OracleDbType.Int64).Value = cnp;
            cmd.CommandText = "SELECT PACIENT FROM RETETA WHERE PACIENT =:CNP";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            reader.Close();
            if (dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool Check_Patient_Not_Exists (string cnp)
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Add("CNP", OracleDbType.Varchar2, 13).Value = cnp;
            cmd.CommandText = "SELECT * FROM PACIENT WHERE CNP =:CNP";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            reader.Close();
            if (dt.Rows.Count > 0 )
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private void ClearPacientFields()
        {
            Pacient_Error.Text = "";
            Pacient_CNP.Clear();
            Pacient_Nume.Clear();
            Pacient_Prenume.Clear();
            Pacient_Varsta.Clear();
            Pacient_TipAsigurare.SelectedItem = null;
            PacientTable.SelectedItem = null;
            Pacient_Actualizeaza_Button.IsEnabled = false;
            Pacient_Sterge_Button.IsEnabled = false;
            Pacient_Adauga_Button.IsEnabled = true;
            Pacient_CNP.IsReadOnly = false;
            updatePacientTable();
        }
        private bool Validate_Pacient_Fields ()
        {
            var cnp = Pacient_CNP.Text.Trim().ToString();
            if (cnp.Length != 13 || !cnp.All(Char.IsDigit))
            {
                Pacient_Error.Text = "CNP-ul trebuie sa fie format din exact 13 cifre!";
                return false;
            }
            var nume = Pacient_Nume.Text.Trim().ToString();
            if (nume.Length > 50 || nume.Length == 0)
            {
                Pacient_Error.Text = "Numele trebuie sa fie format din maxim 50 de caractere!";
                return false;
            }
            var prenume = Pacient_Prenume.Text.Trim().ToString();
            if (prenume.Length > 50 || prenume.Length == 0)
            {
                Pacient_Error.Text = "Numele trebuie sa fie format din maxim 50 de caractere!";
                return false;
            }
            var varstaStr = Pacient_Varsta.Text.Trim().ToString();
            if (varstaStr.Length > 3 || varstaStr.Length == 0 || !varstaStr.All(Char.IsDigit))
            {
                Pacient_Error.Text = "Varsta trebuie sa fie formata din maxim 3 cifre!";
                return false;
            }
            if (Pacient_TipAsigurare.SelectedItem == null)
            {
                Pacient_Error.Text = "Va rugam selectati un tip de asigurare!";
                return false;
            }

            return true;
        }
        private void Pacient_Adauga_Click(object sender, RoutedEventArgs e)
        {
            Pacient_Error.Text = "";
            if (!Validate_Pacient_Fields())
            {
                return;
            }
            var cnp = Pacient_CNP.Text.Trim().ToString();
            if (!Check_Patient_Not_Exists(cnp))
            {
                Pacient_Error.Text = "Persoana cu acest cnp a fost deja adaugata in tabel";
                return;
            }
            var nume = Pacient_Nume.Text.Trim().ToString();
            var prenume = Pacient_Prenume.Text.Trim().ToString();
            var varsta = Int64.Parse(Pacient_Varsta.Text.Trim().ToString());
            var tipAsigurare = Pacient_TipAsigurare.SelectedValue.ToString();

            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT into Pacient(CNP, Nume, Prenume, Varsta, TipAsigurare) " + " VALUES(:CNP, :NUME,:PRENUME,:VARSTA, :TIPASIGURARE)";
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("CNP", OracleDbType.Varchar2, 13).Value = cnp;
            cmd.Parameters.Add("NUME", OracleDbType.Varchar2, 50).Value = nume;
            cmd.Parameters.Add("PRENUME", OracleDbType.Varchar2, 50).Value = prenume;
            cmd.Parameters.Add("VARSTA", OracleDbType.Int64).Value = varsta;
            cmd.Parameters.Add("TIPASIGURARE", OracleDbType.Varchar2, 50).Value = tipAsigurare;
            try
            {
                int n = cmd.ExecuteNonQuery();
                if (n > 0)
                {
                    ClearPacientFields();
                    Pacient_Error.Text = "Intrare adaugata cu success!";
                }
            }
            catch (Exception ex) {
                throw ex;
            }
        }
        private void Pacient_Reset_Click(object sender, RoutedEventArgs e)
        {
            ClearPacientFields();
        }
        private void Pacient_Actualizeaza_Click(object sender, RoutedEventArgs e)
        {
            Pacient_Error.Text = "";
            if (!Validate_Pacient_Fields())
            {
                return;
            }

            var cnp = Pacient_CNP.Text.Trim().ToString();
            if (Check_Patient_Not_Exists(cnp))
            {
                Pacient_Error.Text = "CNP-ul nu exista in baza de date";
                return;
            }

            var nume = Pacient_Nume.Text.Trim().ToString();
            var prenume = Pacient_Prenume.Text.Trim().ToString();
            var varsta = Int64.Parse(Pacient_Varsta.Text.Trim().ToString());
            var tipAsigurare = Pacient_TipAsigurare.SelectedValue.ToString();

            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Clear();
            cmd.CommandText = "UPDATE PACIENT SET NUME = :NUME, PRENUME = :PRENUME, VARSTA = :VARSTA, TIPASIGURARE = :TIPASIGURARE WHERE CNP = :CNP";
       
            cmd.Parameters.Add("NUME", OracleDbType.Varchar2, 50).Value = nume;
            cmd.Parameters.Add("PRENUME", OracleDbType.Varchar2, 50).Value = prenume;
            cmd.Parameters.Add("VARSTA", OracleDbType.Int64).Value = varsta;
            cmd.Parameters.Add("TIPASIGURARE", OracleDbType.Varchar2, 50).Value = tipAsigurare;
            cmd.Parameters.Add("CNP", OracleDbType.Varchar2, 13).Value = cnp;
            try
            {
                int n = cmd.ExecuteNonQuery();
                if (n > 0)
                {
                    ClearPacientFields();
                    Pacient_Error.Text = "Intrare actualizata cu success!";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
         }
        private void Pacient_Sterge_Click(object sender, RoutedEventArgs e)
        {
            var cnp = Pacient_CNP.Text.Trim().ToString();
            if (Check_Patient_Not_Exists (cnp))
            {
                Pacient_Error.Text = "CNP-ul nu exista in baza de date";
                return;
            }

            if (Check_Pacient_Used_In_Reteta(cnp))
            {
                Pacient_Error.Text = "Pacientul cu acest CNP nu poate fi sters deoarece este inregistrat intr-o reteta!";
                return;
            }

            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Add("CNP", OracleDbType.Varchar2, 13).Value = cnp;
            cmd.CommandText = "DELETE FROM PACIENT WHERE CNP =:CNP";
            cmd.CommandType = CommandType.Text;
            try
            {
                int n = cmd.ExecuteNonQuery();
                if (n > 0)
                {
                    ClearPacientFields();
                    Pacient_Error.Text = "Intrare stearsa cu success!";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void PacientTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null) { return; }
            DataGrid dg = sender as DataGrid;
            if (dg == null) { return; }
            DataRowView dr = dg.SelectedItem as DataRowView;
            if (dr == null) { return; } 
            Pacient_CNP.Text = dr["CNP"].ToString();
            Pacient_Nume.Text = dr["NUME"].ToString();
            Pacient_Prenume.Text = dr["PRENUME"].ToString();
            Pacient_Varsta.Text = dr["VARSTA"].ToString();
            Pacient_TipAsigurare.Text = dr["TIPASIGURARE"].ToString();
            Pacient_Actualizeaza_Button.IsEnabled = true;
            Pacient_Sterge_Button.IsEnabled = true;
            Pacient_Adauga_Button.IsEnabled = false;
            Pacient_CNP.IsReadOnly = true;
        }

        #endregion

        #region Diagnostic
        void updateDiagnosticTable()
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT CODBOALA,DENUMIRE,TIP FROM DIAGNOSTIC ORDER BY CODBOALA ASC";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            DiagnosticTable.ItemsSource = dt.DefaultView;
            reader.Close();
            updateTratamentCombobox();
        }
        private bool Check_Diagnostic_Not_Exists(int codBoala)
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Add("CODBOALA", OracleDbType.Int64).Value = codBoala;
            cmd.CommandText = "SELECT * FROM DIAGNOSTIC WHERE CODBOALA =:CODBOALA";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            reader.Close();
            if (dt.Rows.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private bool Check_Diagnostic_Used_In_Tratament(int codBoala)
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Add("CODBOALA", OracleDbType.Int64).Value = codBoala;
            cmd.CommandText = "SELECT CODBOALA FROM TRATAMENT WHERE CODBOALA =:CODBOALA";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            reader.Close();
            if (dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void ClearDiagnosticFields()
        {
            Diagnostic_Error.Text = "";
            Diagnostic_CodBoala.Clear();
            Diagnostic_Denumire.Clear();
            Diagnostic_Tip.SelectedItem = null;
            DiagnosticTable.SelectedItem = null;
            Diagnostic_Actualizeaza_Button.IsEnabled = false;
            Diagnostic_Sterge_Button.IsEnabled = false;
            Diagnostic_Adauga_Button.IsEnabled = true;
            Diagnostic_CodBoala.IsReadOnly = false;
            updateDiagnosticTable();
        }
        private bool Validate_Diagnostic_Fields()
        {
            var codBoala = Diagnostic_CodBoala.Text.Trim().ToString();
            if (codBoala.Length == 0 || codBoala.Length > 3 || !codBoala.All(Char.IsDigit))
            {
                Diagnostic_Error.Text = "Codul trebuie sa fie format din 0-3 cifre!";
                return false;
            }
            var denumire = Diagnostic_Denumire.Text.ToString();
            if (denumire.Length > 50 || denumire.Length == 0)
            {
                Diagnostic_Error.Text = "Denumirea trebuie sa fie formata din 0-100 caractere!";
                return false;
            }
            if (Diagnostic_Tip.SelectedItem == null)
            {
                Diagnostic_Error.Text = "Va rugam selectati un tip de diagnostic!";
                return false;
            }
            return true;
        }
        private void Diagnostic_Adauga_Click(object sender, RoutedEventArgs e)
        {
            Diagnostic_Error.Text = "";
            if (!Validate_Diagnostic_Fields())
            {
                return;
            }
            var codBoala = Int32.Parse(Diagnostic_CodBoala.Text.Trim().ToString());
            if (!Check_Diagnostic_Not_Exists(codBoala))
            {
                Diagnostic_Error.Text = "Diagnosticul cu acest cod a fost deja adaugata in tabel";
                return;
            }
            var denumire = Diagnostic_Denumire.Text.ToString();
            var tip = Diagnostic_Tip.SelectedValue.ToString();

            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT into DIAGNOSTIC(CODBOALA, DENUMIRE, TIP) " + " VALUES(:CODBOALA,:DENUMIRE,:TIP)";
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("CODBOALA", OracleDbType.Int64).Value = codBoala;
            cmd.Parameters.Add("DENUMIRE", OracleDbType.Varchar2, 100).Value = denumire;
            cmd.Parameters.Add("TIP", OracleDbType.Varchar2, 50).Value = tip;
            try
            {
                int n = cmd.ExecuteNonQuery();
                if (n > 0)
                {
                    ClearDiagnosticFields();
                    Diagnostic_Error.Text = "Intrare adaugata cu success!";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void Diagnostic_Reset_Click(object sender, RoutedEventArgs e)
        {
            ClearDiagnosticFields();
        }
        private void Diagnostic_Actualizeaza_Click(object sender, RoutedEventArgs e)
        {
            Diagnostic_Error.Text = "";
            if (!Validate_Diagnostic_Fields())
            {
                return;
            }
            var codBoala = Int32.Parse(Diagnostic_CodBoala.Text.Trim().ToString());
            if (Check_Diagnostic_Not_Exists(codBoala))
            {
                Diagnostic_Error.Text = "Diagnosticul cu acest nu exista in baza de date!";
                return;
            }
            var denumire = Diagnostic_Denumire.Text.ToString();
            var tip = Diagnostic_Tip.SelectedValue.ToString();

            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Clear();
            cmd.CommandText = "UPDATE DIAGNOSTIC SET DENUMIRE = :DENUMIRE, TIP = :TIP WHERE CODBOALA = :CODBOALA";
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("DENUMIRE", OracleDbType.Varchar2, 100).Value = denumire;
            cmd.Parameters.Add("TIP", OracleDbType.Varchar2, 50).Value = tip;
            cmd.Parameters.Add("CODBOALA", OracleDbType.Int64).Value = codBoala;
            try
            {
                int n = cmd.ExecuteNonQuery();
                if (n > 0)
                {
                    ClearDiagnosticFields();
                    Diagnostic_Error.Text = "Intrare actualizata cu success!";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void Diagnostic_Sterge_Click(object sender, RoutedEventArgs e)
        {
            var codBoala = Int32.Parse(Diagnostic_CodBoala.Text.Trim().ToString());
            if (Check_Diagnostic_Not_Exists(codBoala))
            {
                Diagnostic_Error.Text = "Diagnosticul cu acest cod nu exista in baza de date!";
                return;
            }
            if (Check_Diagnostic_Used_In_Tratament(codBoala))
            {
                Diagnostic_Error.Text = "Diagnosticul cu acest cod nu poate fi sters deoarece este folosit intr-un tratament!";
                return;
            }

            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Add("CODBOALA", OracleDbType.Int64).Value = codBoala;
            cmd.CommandText = "DELETE FROM DIAGNOSTIC WHERE CODBOALA =:CODBOALA";
            cmd.CommandType = CommandType.Text;
            try
            {
                int n = cmd.ExecuteNonQuery();
                if (n > 0)
                {
                    ClearDiagnosticFields();
                    Diagnostic_Error.Text = "Intrare stearsa cu success!";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void DiagnosticTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null) { return; }
            DataGrid dg = sender as DataGrid;
            if (dg == null) { return; }
            DataRowView dr = dg.SelectedItem as DataRowView;
            if (dr == null) { return; }
            Diagnostic_CodBoala.Text = dr["CODBOALA"].ToString();
            Diagnostic_Denumire.Text = dr["DENUMIRE"].ToString();
            Diagnostic_Tip.Text = dr["TIP"].ToString();
            Diagnostic_Actualizeaza_Button.IsEnabled = true;
            Diagnostic_Sterge_Button.IsEnabled = true;
            Diagnostic_Adauga_Button.IsEnabled = false;
            Diagnostic_CodBoala.IsReadOnly = true;
        }
        #endregion

        #region Tratament
        private void updateTratamentCombobox ()
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT CodBoala FROM Diagnostic ORDER BY CodBoala ASC";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            Tratament_CodBoala.Items.Clear();
            while (reader.Read())
            {
                Tratament_CodBoala.Items.Add(reader[0].ToString());
            };
        }
        private void updateTratamentTable()
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Pozitie, CodBoala, CodMedicament, DenumireMedicament, Cantitate FROM TRATAMENT ORDER BY Pozitie ASC";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            TratamentTable.ItemsSource = dt.DefaultView;
            reader.Close();
            updateTratamentCombobox ();
            updateTRPoziteCombobox();
            updateTRNumarCombobox();
        }
        private bool Check_Tratament_Not_Exists(int pozitie)
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Add("POZITIE", OracleDbType.Int64).Value = pozitie;
            cmd.CommandText = "SELECT * FROM TRATAMENT WHERE POZITIE =:POZITIE";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            reader.Close();
            if (dt.Rows.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private bool Check_Tratament_Is_Used_In_TR(int pozitie)
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Add("POZITIETRATAMENT", OracleDbType.Int64).Value = pozitie;
            cmd.CommandText = "SELECT * FROM TRATAMENT_RETETA WHERE POZITIETRATAMENT =:POZITIETRATAMENT";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            reader.Close();
            if (dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void ClearTratamentFields()
        {
            Tratament_Error.Text = "";
            Tratament_CodMedicament.Clear();
            Tratament_DenumireMedicament.Clear();
            Tratament_Pozitie.Clear();
            Tratament_Cantitate.Clear();
            Tratament_CodBoala.SelectedItem = null;
            TratamentTable.SelectedItem = null;
            Tratament_Actualizeaza_Button.IsEnabled = false;
            Tratament_Sterge_Button.IsEnabled = false;
            Tratament_Adauga_Button.IsEnabled = true;
            Tratament_Pozitie.IsReadOnly = false;
            updateTratamentTable();
        }
        private bool Validate_Tratament_Fields()
        {
            var pozitie = Tratament_Pozitie.Text.Trim().ToString();
            if (pozitie.Length == 0 || pozitie.Length > 8 || !pozitie.All(Char.IsDigit))
            {
                Tratament_Error.Text = "Pozitia trebuie sa fie formata din 1-8 cifre!";
                return false;
            }
            if (Tratament_CodBoala.SelectedItem == null)
            {
                Tratament_Error.Text = "Va rugam selectati un cod de boala";
                return false;
            }
            var codMedicament = Tratament_CodMedicament.Text.Trim().ToString();
            if (codMedicament.Length != 6)
            {
                Tratament_Error.Text = "CodMedicament trebuie sa aibe formatul 'A00000' !";
                return false;
            }
            var checkNumber = codMedicament[1].ToString() + codMedicament[2].ToString() + codMedicament[3].ToString() + codMedicament[4].ToString() + codMedicament[5].ToString();
            if (!(codMedicament[0] >= 'A' && codMedicament[0] <= 'Z') || !checkNumber.All(Char.IsDigit))
            {
                Tratament_Error.Text = "CodMedicament trebuie sa aibe formatul 'A00000' !";
                return false;
            }
            var denumireMedicament = Tratament_DenumireMedicament.Text.ToString();
            if (denumireMedicament.Length > 100 || denumireMedicament.Length == 0)
            {
                Tratament_Error.Text = "Denumirea medicamentului trebuie sa fie formata din 1-100 caractere!";
                return false;
            }
            var cantitate = Tratament_Cantitate.Text.Trim().ToString();
            if (cantitate.Length > 8 || cantitate.Length == 0 || !cantitate.All(Char.IsDigit))
            {
                Tratament_Error.Text = "Cantitatea trebuie sa fie formata din 1-9 cifre!";
                return false;
            }
            return true;
        }
        private void Tratament_Adauga_Click(object sender, RoutedEventArgs e)
        {
            Tratament_Error.Text = "";
            if (!Validate_Tratament_Fields())
            {
                return;
            }
            var pozitie = Int32.Parse(Tratament_Pozitie.Text.Trim().ToString());
            if (!Check_Tratament_Not_Exists(pozitie))
            {
                Tratament_Error.Text = "Tratamentul cu acesta pozitie a fost deja adaugat in tabel!";
                return;
            }
            var denumireMedicament = Tratament_DenumireMedicament.Text.ToString();
            var codMedicament = Tratament_CodMedicament.Text.Trim().ToString();
            var cantitate = Int32.Parse(Tratament_Cantitate.Text.Trim().ToString());
            var codBoala = Int32.Parse(Tratament_CodBoala.SelectedItem.ToString());

            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT into Tratament(POZITIE, CODBOALA, CODMEDICAMENT, DENUMIREMEDICAMENT, CANTITATE) " + " VALUES(:POZITIE, :CODBOALA, :CODMEDICAMENT, :DENUMIREMEDICAMENT, :CANTITATE)";
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("POZITIE", OracleDbType.Int32).Value = pozitie;
            cmd.Parameters.Add("CODBOALA", OracleDbType.Int32).Value = codBoala;
            cmd.Parameters.Add("CODMEDICAMENT", OracleDbType.Varchar2, 6).Value = codMedicament;
            cmd.Parameters.Add("DENUMIREMEDICAMENT", OracleDbType.Varchar2, 100).Value = denumireMedicament;
            cmd.Parameters.Add("CANTITATE", OracleDbType.Int32).Value = cantitate;
            try
            {
                int n = cmd.ExecuteNonQuery();
                if (n > 0)
                {
                    ClearTratamentFields();
                    Tratament_Error.Text = "Intrare adaugata cu success!";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void Tratament_Reset_Click(object sender, RoutedEventArgs e)
        {
            ClearTratamentFields();
        }
        private void Tratament_Actualizeaza_Click(object sender, RoutedEventArgs e)
        {
            Tratament_Error.Text = "";
            if (!Validate_Tratament_Fields())
            {
                return;
            }

            var pozitie = Int32.Parse(Tratament_Pozitie.Text.Trim().ToString());
            if (Check_Tratament_Not_Exists(pozitie))
            {
                Tratament_Error.Text = "Tratamentul cu acesta pozitie nu exista in tabel!";
                return;
            }

            var denumireMedicament = Tratament_DenumireMedicament.Text.ToString();
            var codMedicament = Tratament_CodMedicament.Text.Trim().ToString();
            var cantitate = Int32.Parse(Tratament_Cantitate.Text.Trim().ToString());
            var codBoala = Int32.Parse(Tratament_CodBoala.SelectedItem.ToString());

            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Clear();
            cmd.CommandText = "UPDATE TRATAMENT SET CODBOALA = :CODBOALA, CODMEDICAMENT = :CODMEDICAMENT, DENUMIREMEDICAMENT = :DENUMIREMEDICAMENT, CANTITATE = :CANTITATE WHERE POZITIE = :POZITIE";

            cmd.Parameters.Add("CODBOALA", OracleDbType.Int32).Value = codBoala;
            cmd.Parameters.Add("CODMEDICAMENT", OracleDbType.Varchar2, 6).Value = codMedicament;
            cmd.Parameters.Add("DENUMIREMEDICAMENT", OracleDbType.Varchar2, 100).Value = denumireMedicament;
            cmd.Parameters.Add("CANTITATE", OracleDbType.Int32).Value = cantitate;
            cmd.Parameters.Add("POZITIE", OracleDbType.Int32).Value = pozitie;
            try
            {
                int n = cmd.ExecuteNonQuery();
                if (n > 0)
                {
                    ClearTratamentFields();
                    Tratament_Error.Text = "Intrare actualizata cu success!";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void Tratament_Sterge_Click(object sender, RoutedEventArgs e)
        {
            var pozitie = Int32.Parse(Tratament_Pozitie.Text.Trim().ToString());
            if (Check_Tratament_Not_Exists(pozitie))
            {
                Tratament_Error.Text = "Tratamentul cu acesta pozitie nu exista in tabel!";
                return;
            }
            if (Check_Tratament_Is_Used_In_TR(pozitie))
            {
                Tratament_Error.Text = "Tratamentul este asociat unei retete si nu poate fi sters!";
                return;
            }
            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Add("POZITIE", OracleDbType.Int32).Value = pozitie;
            cmd.CommandText = "DELETE FROM TRATAMENT WHERE POZITIE =:POZITIE";
            cmd.CommandType = CommandType.Text;
            try
            {
                int n = cmd.ExecuteNonQuery();
                if (n > 0)
                {
                    ClearTratamentFields();
                    Tratament_Error.Text = "Intrare stearsa cu success!";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void TratamentTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null) { return; }
            DataGrid dg = sender as DataGrid;
            if (dg == null) { return; }
            DataRowView dr = dg.SelectedItem as DataRowView;
            if (dr == null) { return; }
            Tratament_Pozitie.Text = dr["POZITIE"].ToString();
            Tratament_CodMedicament.Text = dr["CODMEDICAMENT"].ToString();
            Tratament_DenumireMedicament.Text = dr["DENUMIREMEDICAMENT"].ToString();
            Tratament_Cantitate.Text = dr["CANTITATE"].ToString();
            Tratament_CodBoala.Text = dr["CODBOALA"].ToString();
            Tratament_Actualizeaza_Button.IsEnabled = true;
            Tratament_Sterge_Button.IsEnabled = true;
            Tratament_Adauga_Button.IsEnabled = false;
            Tratament_Pozitie.IsReadOnly = true;
        }
        #endregion

        #region Reteta
        private void updateRetetaCombobox()
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT CNP FROM PACIENT ORDER BY CNP ASC";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            Reteta_Pacient.Items.Clear();
            while (reader.Read())
            {
                Reteta_Pacient.Items.Add(reader[0]);
            };
        }
        private void updateRetetaTable()
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT NumarReteta, Pacient, UnitateMedicala, Judet, Medic FROM Reteta ORDER BY NumarReteta ASC";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            RetetaTable.ItemsSource = dt.DefaultView;
            reader.Close();
            updateRetetaCombobox();
            updateTRPoziteCombobox();
            updateTRNumarCombobox();
        }
        private bool Check_Reteta_Not_Exists(string numarReteta)
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Add("NUMARRETETA", OracleDbType.Varchar2, 20).Value = numarReteta;
            cmd.CommandText = "SELECT * FROM RETETA WHERE NUMARRETETA =:NUMARRETETA";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            reader.Close();
            if (dt.Rows.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private bool Check_Reteta_Is_Used_In_TR (string numarReteta)
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Add("NUMARRETETA", OracleDbType.Varchar2, 20).Value = numarReteta;
            cmd.CommandText = "SELECT * FROM TRATAMENT_RETETA WHERE NUMARRETETA =:NUMARRETETA";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            reader.Close();
            if (dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void ClearRetetaFields()
        {
            Reteta_Error.Text = "";
            Reteta_NumarReteta.Clear();
            Reteta_Pacient.SelectedItem = null;
            Reteta_Medic.Clear();
            Reteta_UnitateMedicala.Clear();
            Reteta_Judet.SelectedItem = null;
            RetetaTable.SelectedItem = null;
            Reteta_Actualizeaza_Button.IsEnabled = false;
            Reteta_Sterge_Button.IsEnabled = false;
            Reteta_Adauga_Button.IsEnabled = true;
            Reteta_NumarReteta.IsReadOnly = false;
            updateRetetaTable();
        }
        private bool Validate_Reteta_Fields()
        {
            var numarReteta = Reteta_NumarReteta.Text.Trim().ToString();
            var regex = new Regex(@"\d{6}/\d{2}.\d{2}.\d{4}");
            Match match = regex.Match(numarReteta);
            if (!match.Success)
            {
                Reteta_Error.Text = "Numar reteta trebuie sa aibe formatul 000000/00.00.0000 !";
                return false;
            }
            if (Reteta_Pacient.SelectedItem == null)
            {
                Reteta_Error.Text = "Va rugam selectati un pacient!";
                return false;
            }
            var unitateMedicala = Reteta_UnitateMedicala.Text.Trim().ToString();
            if (unitateMedicala.Length > 100 || unitateMedicala.Length == 0)
            {
                Reteta_Error.Text = "Numele unitatii medicale trebuie sa fie format din 1-100 caractere!";
                return false;
            }
            if (Reteta_Judet.SelectedItem == null)
            {
                Reteta_Error.Text = "Va rugam selectati un judet!";
                return false;
            }
            var medic = Reteta_Medic.Text.Trim();
            regex = new Regex(@"ME\d{6}");
            match = regex.Match(medic);
            if (!match.Success) {
                Reteta_Error.Text = "Cod medic trebuie sa aibe formatul ME000000!";
                return false;
            }
            return true;
        }
        private void Reteta_Adauga_Click(object sender, RoutedEventArgs e)
        {
            Pacient_Error.Text = "";
            if (!Validate_Reteta_Fields())
            {
                return;
            }

            var numarReteta = Reteta_NumarReteta.Text.Trim().ToString();
            if (!Check_Reteta_Not_Exists(numarReteta))
            {
                Reteta_Error.Text = "Reteta cu acest numar a fost deja adaugata in tabel";
                return;
            }

            var unitateMedicala = Reteta_UnitateMedicala.Text.Trim().ToString();
            var medic = Reteta_Medic.Text.Trim();
            var judet = Reteta_Judet.SelectedValue.ToString();
            var pacient = Reteta_Pacient.SelectedItem.ToString();

            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT into RETETA (NUMARRETETA, UNITATEMEDICALA, JUDET, MEDIC, PACIENT) " + " VALUES(:NUMARRETETA, :UNITATEMEDICALA, :JUDET,:MEDIC, :PACIENT)";
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("NUMARRETETA", OracleDbType.Varchar2, 20).Value = numarReteta;
            cmd.Parameters.Add("UNITATEMEDICALA", OracleDbType.Varchar2, 100).Value = unitateMedicala;
            cmd.Parameters.Add("JUDET", OracleDbType.Varchar2, 50).Value = judet;
            cmd.Parameters.Add("MEDIC", OracleDbType.Varchar2,8).Value = medic;
            cmd.Parameters.Add("PACIENT", OracleDbType.Varchar2, 13).Value = pacient;
            try
            {
                int n = cmd.ExecuteNonQuery();
                if (n > 0)
                {
                    ClearRetetaFields();
                    Reteta_Error.Text = "Intrare adaugata cu success!";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void Reteta_Reset_Click(object sender, RoutedEventArgs e)
        {
            ClearRetetaFields();
        }
        private void Reteta_Actualizeaza_Click(object sender, RoutedEventArgs e)
        {
            Pacient_Error.Text = "";
            if (!Validate_Reteta_Fields())
            {
                return;
            }

            var numarReteta = Reteta_NumarReteta.Text.Trim().ToString();
            if (Check_Reteta_Not_Exists(numarReteta))
            {
                Reteta_Error.Text = "Reteta cu acest numar nu se afla in tabel";
                return;
            }

            var unitateMedicala = Reteta_UnitateMedicala.Text.Trim().ToString();
            var medic = Reteta_Medic.Text.Trim();
            var judet = Reteta_Judet.SelectedValue.ToString();
            var pacient = Reteta_Pacient.SelectedItem.ToString();

            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Clear();
            cmd.CommandText = "UPDATE RETETA SET UNITATEMEDICALA = :UNITATEMEDICALA, JUDET = :JUDET, MEDIC = :MEDIC, PACIENT = :PACIENT WHERE NUMARRETETA = :NUMARRETETA";

            cmd.Parameters.Add("UNITATEMEDICALA", OracleDbType.Varchar2, 100).Value = unitateMedicala;
            cmd.Parameters.Add("JUDET", OracleDbType.Varchar2, 50).Value = judet;
            cmd.Parameters.Add("MEDIC", OracleDbType.Varchar2, 8).Value = medic;
            cmd.Parameters.Add("PACIENT", OracleDbType.Varchar2, 13).Value = pacient;
            cmd.Parameters.Add("NUMARRETETA", OracleDbType.Varchar2, 20).Value = numarReteta;

            try
            {
                int n = cmd.ExecuteNonQuery();
                if (n > 0)
                {
                    ClearRetetaFields();
                    Reteta_Error.Text = "Intrare actualizata cu success!";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void Reteta_Sterge_Click(object sender, RoutedEventArgs e)
        {
            var numarReteta = Reteta_NumarReteta.Text.Trim().ToString();
            if (Check_Reteta_Not_Exists(numarReteta))
            {
                Reteta_Error.Text = "Acest numar de reteta nu exista in baza de date";
                return;
            }
            if (Check_Reteta_Is_Used_In_TR(numarReteta))
            {
                Reteta_Error.Text = "Acesta reteta are tratamente asociate deci nu poate fi stearsa!";
                return;
            }
            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Add("NUMARRETETA", OracleDbType.Varchar2, 20).Value = numarReteta;
            cmd.CommandText = "DELETE FROM RETETA WHERE NUMARRETETA =:NUMARRETETA";
            cmd.CommandType = CommandType.Text;
            try
            {
                int n = cmd.ExecuteNonQuery();
                if (n > 0)
                {
                    ClearRetetaFields();
                    Reteta_Error.Text = "Intrare stearsa cu success!";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void RetetaTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null) { return; }
            DataGrid dg = sender as DataGrid;
            if (dg == null) { return; }
            DataRowView dr = dg.SelectedItem as DataRowView;
            if (dr == null) { return; }
            Reteta_NumarReteta.Text = dr["NUMARRETETA"].ToString();
            Reteta_Judet.Text = dr["JUDET"].ToString();
            Reteta_Medic.Text = dr["MEDIC"].ToString();
            Reteta_Pacient.Text = dr["PACIENT"].ToString();
            Reteta_UnitateMedicala.Text = dr["UNITATEMEDICALA"].ToString();
            Reteta_Actualizeaza_Button.IsEnabled = true;
            Reteta_Sterge_Button.IsEnabled = true;
            Reteta_Adauga_Button.IsEnabled = false;
            Reteta_NumarReteta.IsReadOnly = true;
        }

        #endregion

        #region TR
        private void updateTRPoziteCombobox()
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT POZITIE FROM TRATAMENT ORDER BY POZITIE ASC";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            TR_PozitieTratament.Items.Clear();
            while (reader.Read())
            {
                TR_PozitieTratament.Items.Add(reader[0].ToString());
            };
        }
        private void updateTRNumarCombobox()
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT NUMARRETETA FROM RETETA ORDER BY NUMARRETETA ASC";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            TR_NumarReteta.Items.Clear();
            while (reader.Read())
            {
                TR_NumarReteta.Items.Add(reader[0]);
            };
        }
        private void updateTRTable ()
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT NUMARRETETA,POZITIETRATAMENT FROM TRATAMENT_RETETA ORDER BY NUMARRETETA,POZITIETRATAMENT ASC";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            TRTable.ItemsSource = dt.DefaultView;
            reader.Close();
            updateTRPoziteCombobox();
            updateTRNumarCombobox();
        }
        private bool Check_TR_Not_Exists(string numarReteta, int pozitie)
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Add("NUMARRETETA", OracleDbType.Varchar2, 20).Value = numarReteta;
            cmd.Parameters.Add("POZITIETRATAMENT", OracleDbType.Int64).Value = pozitie;
            cmd.CommandText = "SELECT * FROM TRATAMENT_RETETA WHERE NUMARRETETA =:NUMARRETETA AND POZITIETRATAMENT=:POZITIETRATAMENT";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            reader.Close();
            if (dt.Rows.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private void ClearTRFields()
        {
            TR_Error.Text = "";
            TR_NumarReteta.SelectedItem = null;
            TR_PozitieTratament.SelectedItem = null;
            TRTable.SelectedItem = null;
            TR_Sterge_Button.IsEnabled = false;
            TR_Adauga_Button.IsEnabled = true;
            TR_NumarReteta.IsEnabled = true;
            TR_PozitieTratament.IsEnabled = true;
            updateTRTable();
        }
        private bool Validate_TR_Fields ()
        {
            if (TR_NumarReteta.SelectedItem == null)
            {
                TR_Error.Text = "Va rugam selectati o reteta!";
                return false;
            }
            if (TR_PozitieTratament.SelectedItem == null)
            {
                TR_Error.Text = "Va rugam selectati un tratament!";
                return false;
            }
            return true;
        }
        private void TR_Adauga_Click(object sender, RoutedEventArgs e)
        {
            TR_Error.Text = "";
            if (!Validate_TR_Fields())
            {
                return;
            }

            var numarReteta = TR_NumarReteta.SelectedItem.ToString();
            var pozitieTratament = Int32.Parse(TR_PozitieTratament.SelectedItem.ToString());
            if (!Check_TR_Not_Exists(numarReteta, pozitieTratament))
            {
                TR_Error.Text = "Acest tratament a fost deja adaugat la aceeasta reteta";
                return;
            }

            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT into TRATAMENT_RETETA (NUMARRETETA, POZITIETRATAMENT) " + " VALUES(:NUMARRETETA, :POZITIETRATAMENT)";
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("NUMARRETETA", OracleDbType.Varchar2, 20).Value = numarReteta;
            cmd.Parameters.Add("POZITIETRATAMENT", OracleDbType.Int64).Value = pozitieTratament;
            try
            {
                int n = cmd.ExecuteNonQuery();
                if (n > 0)
                {
                    ClearTRFields();
                    TR_Error.Text = "Intrare adaugata cu success!";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private void TR_Reset_Click(object sender, RoutedEventArgs e)
        {
            ClearTRFields();
        }
        private void TR_Sterge_Click(object sender, RoutedEventArgs e)
        {
            var numarReteta = TR_NumarReteta.SelectedItem.ToString();
            var pozitieTratament = Int32.Parse(TR_PozitieTratament.SelectedItem.ToString());
            if (Check_TR_Not_Exists(numarReteta, pozitieTratament))
            {
                TR_Error.Text = "Acest acest tratamentul nu este adaugat la reteta in cauza!";
                return;
            }

            OracleCommand cmd = connection.CreateCommand();
            cmd.Parameters.Add("NUMARRETETA", OracleDbType.Varchar2, 20).Value = numarReteta;
            cmd.Parameters.Add("POZITIETRATAMENT", OracleDbType.Int64).Value = pozitieTratament;
            cmd.CommandText = "DELETE FROM TRATAMENT_RETETA WHERE NUMARRETETA =:NUMARRETETA AND POZITIETRATAMENT=:POZITIETRATAMENT";
            cmd.CommandType = CommandType.Text;
            try
            {
                int n = cmd.ExecuteNonQuery();
                if (n > 0)
                {
                    ClearTRFields();
                    TR_Error.Text = "Intrare stearsa cu success!";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void TRTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null) { return; }
            DataGrid dg = sender as DataGrid;
            if (dg == null) { return; }
            DataRowView dr = dg.SelectedItem as DataRowView;
            if (dr == null) { return; }
            TR_NumarReteta.Text = dr["NUMARRETETA"].ToString();
            TR_PozitieTratament.Text = dr["POZITIETRATAMENT"].ToString();
            TR_Sterge_Button.IsEnabled = true;
            TR_Adauga_Button.IsEnabled = false;
            TR_NumarReteta.IsEnabled = false;
            TR_PozitieTratament.IsEnabled = false;
        }

        #endregion

        #region Statistici
        private void CalculeazaStatisticaReteta_Click(object sender, RoutedEventArgs e)
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT  CNP, NUME, PRENUME, (SELECT count(*) FROM RETETA where PACIENT = PACIENT.CNP ) AS NUMARRETETE FROM PACIENT";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            StatisticiTable.ItemsSource = dt.DefaultView;
            reader.Close();
            Statistici_Error.Text = "A fost calculat numarul de retete pentru fiecare pacient inregistrat.";
        }
        private void CalculeazaStatisticaMedicamente_Click(object sender, RoutedEventArgs e)
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT CODMEDICAMENT, SUM (CANTITATE) FROM (SELECT RETETA.NUMARRETETA,TRATAMENT.CODBOALA, TRATAMENT.CODMEDICAMENT, tratament.denumiremedicament, tratament.cantitate FROM TRATAMENT_RETETA INNER JOIN TRATAMENT ON TRATAMENT_RETETA.POZITIETRATAMENT = TRATAMENT.POZITIE INNER JOIN RETETA ON TRATAMENT_RETETA.NUMARRETETA = RETETA.NUMARRETETA) GROUP BY (CODMEDICAMENT)";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            StatisticiTable.ItemsSource = dt.DefaultView;
            reader.Close();
            Statistici_Error.Text = "A fost calculata cantitatea prescrisa din fiecare medicament.";
        }
        private void CalculeazaStatisticaBoli_Click(object sender, RoutedEventArgs e)
        {
            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT  CODBOALA, DENUMIRE, (SELECT count(*) FROM (SELECT RETETA.NUMARRETETA,TRATAMENT.CODBOALA FROM TRATAMENT_RETETA INNER JOIN TRATAMENT ON TRATAMENT_RETETA.POZITIETRATAMENT = TRATAMENT.POZITIE INNER JOIN RETETA ON TRATAMENT_RETETA.NUMARRETETA = RETETA.NUMARRETETA) where CODBOALA = DIAGNOSTIC.CODBOALA ) AS APARITII FROM DIAGNOSTIC";
            cmd.CommandType = CommandType.Text;
            OracleDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            StatisticiTable.ItemsSource = dt.DefaultView;
            reader.Close();
            Statistici_Error.Text = "A fost calculat numarul de aparitii intr-o reteta a fiecarei boli diagnosticate.";
        }
        #endregion
    }
}