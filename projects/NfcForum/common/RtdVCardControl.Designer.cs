/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 15/03/2012
 * Heure: 09:35
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
namespace SpringCardApplication
{
  partial class RtdVCardControl
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    
    /// <summary>
    /// Disposes resources used by the control.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (components != null) {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }
    
    /// <summary>
    /// This method is required for Windows Forms designer support.
    /// Do not change the method contents inside the source code editor. The Forms designer might
    /// not be able to load this method if it was changed manually.
    /// </summary>
    private void InitializeComponent()
    {
      this.pTitle = new System.Windows.Forms.Panel();
      this.label19 = new System.Windows.Forms.Label();
      this.pContact = new System.Windows.Forms.Panel();
      this.lPSize = new System.Windows.Forms.Label();
      this.bRemovePicture = new System.Windows.Forms.Button();
      this.bChangePicture = new System.Windows.Forms.Button();
      this.flpPicture = new System.Windows.Forms.FlowLayoutPanel();
      this.Nickname = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.Email_alternative = new System.Windows.Forms.TextBox();
      this.Pager = new System.Windows.Forms.TextBox();
      this.label8 = new System.Windows.Forms.Label();
      this.Fax = new System.Windows.Forms.TextBox();
      this.label9 = new System.Windows.Forms.Label();
      this.Home_phone = new System.Windows.Forms.TextBox();
      this.Cell_phone = new System.Windows.Forms.TextBox();
      this.label11 = new System.Windows.Forms.Label();
      this.First_name = new System.Windows.Forms.TextBox();
      this.label12 = new System.Windows.Forms.Label();
      this.label13 = new System.Windows.Forms.Label();
      this.Family_name = new System.Windows.Forms.TextBox();
      this.label14 = new System.Windows.Forms.Label();
      this.label15 = new System.Windows.Forms.Label();
      this.label16 = new System.Windows.Forms.Label();
      this.Business_phone = new System.Windows.Forms.TextBox();
      this.Email = new System.Windows.Forms.TextBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.label1 = new System.Windows.Forms.Label();
      this.pBusiness = new System.Windows.Forms.Panel();
      this.Role_label = new System.Windows.Forms.Label();
      this.Role = new System.Windows.Forms.TextBox();
      this.Title_label = new System.Windows.Forms.Label();
      this.Title = new System.Windows.Forms.TextBox();
      this.Company_label = new System.Windows.Forms.Label();
      this.Company = new System.Windows.Forms.TextBox();
      this.country_company_label = new System.Windows.Forms.Label();
      this.Pro_Country = new System.Windows.Forms.TextBox();
      this.label10 = new System.Windows.Forms.Label();
      this.Pro_Post_Code = new System.Windows.Forms.TextBox();
      this.region_state_company_label = new System.Windows.Forms.Label();
      this.Pro_Region_State = new System.Windows.Forms.TextBox();
      this.town_locality_company_label = new System.Windows.Forms.Label();
      this.Adress_company_label = new System.Windows.Forms.Label();
      this.Pro_Town = new System.Windows.Forms.TextBox();
      this.Pro_Address2 = new System.Windows.Forms.TextBox();
      this.Pro_Address1 = new System.Windows.Forms.TextBox();
      this.panel3 = new System.Windows.Forms.Panel();
      this.label2 = new System.Windows.Forms.Label();
      this.pPrivate = new System.Windows.Forms.Panel();
      this.Birthday_label = new System.Windows.Forms.Label();
      this.Birthday = new System.Windows.Forms.TextBox();
      this.label6 = new System.Windows.Forms.Label();
      this.Country = new System.Windows.Forms.TextBox();
      this.PotsCode_label = new System.Windows.Forms.Label();
      this.Post_Code = new System.Windows.Forms.TextBox();
      this.region_state_tab = new System.Windows.Forms.Label();
      this.Region_State = new System.Windows.Forms.TextBox();
      this.Town_locality_label = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.Town = new System.Windows.Forms.TextBox();
      this.Address2 = new System.Windows.Forms.TextBox();
      this.Address1 = new System.Windows.Forms.TextBox();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.pTitle.SuspendLayout();
      this.pContact.SuspendLayout();
      this.panel1.SuspendLayout();
      this.pBusiness.SuspendLayout();
      this.panel3.SuspendLayout();
      this.pPrivate.SuspendLayout();
      this.SuspendLayout();
      // 
      // pTitle
      // 
      this.pTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(174)))), ((int)(((byte)(141)))), ((int)(((byte)(128)))));
      this.pTitle.Controls.Add(this.label19);
      this.pTitle.Dock = System.Windows.Forms.DockStyle.Top;
      this.pTitle.Location = new System.Drawing.Point(3, 3);
      this.pTitle.Name = "pTitle";
      this.pTitle.Size = new System.Drawing.Size(714, 30);
      this.pTitle.TabIndex = 30;
      // 
      // label19
      // 
      this.label19.AutoSize = true;
      this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label19.ForeColor = System.Drawing.Color.White;
      this.label19.Location = new System.Drawing.Point(10, 7);
      this.label19.Name = "label19";
      this.label19.Size = new System.Drawing.Size(67, 18);
      this.label19.TabIndex = 20;
      this.label19.Text = "Contact";
      // 
      // pContact
      // 
      this.pContact.BackColor = System.Drawing.SystemColors.Window;
      this.pContact.Controls.Add(this.lPSize);
      this.pContact.Controls.Add(this.bRemovePicture);
      this.pContact.Controls.Add(this.bChangePicture);
      this.pContact.Controls.Add(this.flpPicture);
      this.pContact.Controls.Add(this.Nickname);
      this.pContact.Controls.Add(this.label5);
      this.pContact.Controls.Add(this.label7);
      this.pContact.Controls.Add(this.Email_alternative);
      this.pContact.Controls.Add(this.Pager);
      this.pContact.Controls.Add(this.label8);
      this.pContact.Controls.Add(this.Fax);
      this.pContact.Controls.Add(this.label9);
      this.pContact.Controls.Add(this.Home_phone);
      this.pContact.Controls.Add(this.Cell_phone);
      this.pContact.Controls.Add(this.label11);
      this.pContact.Controls.Add(this.First_name);
      this.pContact.Controls.Add(this.label12);
      this.pContact.Controls.Add(this.label13);
      this.pContact.Controls.Add(this.Family_name);
      this.pContact.Controls.Add(this.label14);
      this.pContact.Controls.Add(this.label15);
      this.pContact.Controls.Add(this.label16);
      this.pContact.Controls.Add(this.Business_phone);
      this.pContact.Controls.Add(this.Email);
      this.pContact.Dock = System.Windows.Forms.DockStyle.Top;
      this.pContact.Location = new System.Drawing.Point(3, 33);
      this.pContact.Name = "pContact";
      this.pContact.Size = new System.Drawing.Size(714, 249);
      this.pContact.TabIndex = 31;
      // 
      // lPSize
      // 
      this.lPSize.Location = new System.Drawing.Point(469, 104);
      this.lPSize.Name = "lPSize";
      this.lPSize.Size = new System.Drawing.Size(90, 20);
      this.lPSize.TabIndex = 80;
      this.lPSize.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // bRemovePicture
      // 
      this.bRemovePicture.Location = new System.Drawing.Point(469, 155);
      this.bRemovePicture.Name = "bRemovePicture";
      this.bRemovePicture.Size = new System.Drawing.Size(90, 23);
      this.bRemovePicture.TabIndex = 12;
      this.bRemovePicture.Text = "Remove";
      this.bRemovePicture.UseVisualStyleBackColor = true;
      this.bRemovePicture.Click += new System.EventHandler(this.BRemovePictureClick);
      // 
      // bChangePicture
      // 
      this.bChangePicture.Location = new System.Drawing.Point(469, 126);
      this.bChangePicture.Name = "bChangePicture";
      this.bChangePicture.Size = new System.Drawing.Size(90, 23);
      this.bChangePicture.TabIndex = 11;
      this.bChangePicture.Text = "Choose picture";
      this.bChangePicture.UseVisualStyleBackColor = true;
      this.bChangePicture.Click += new System.EventHandler(this.BChangePictureClick);
      // 
      // flpPicture
      // 
      this.flpPicture.Location = new System.Drawing.Point(469, 11);
      this.flpPicture.Name = "flpPicture";
      this.flpPicture.Size = new System.Drawing.Size(90, 90);
      this.flpPicture.TabIndex = 67;
      // 
      // Nickname
      // 
      this.Nickname.Location = new System.Drawing.Point(116, 63);
      this.Nickname.Name = "Nickname";
      this.Nickname.Size = new System.Drawing.Size(333, 20);
      this.Nickname.TabIndex = 3;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 218);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(97, 20);
      this.label5.TabIndex = 77;
      this.label5.Text = "Alternative e-mail:";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(328, 192);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(51, 20);
      this.label7.TabIndex = 76;
      this.label7.Text = "Pager:";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Email_alternative
      // 
      this.Email_alternative.Location = new System.Drawing.Point(116, 219);
      this.Email_alternative.Name = "Email_alternative";
      this.Email_alternative.Size = new System.Drawing.Size(443, 20);
      this.Email_alternative.TabIndex = 10;
      // 
      // Pager
      // 
      this.Pager.Location = new System.Drawing.Point(382, 193);
      this.Pager.Name = "Pager";
      this.Pager.Size = new System.Drawing.Size(177, 20);
      this.Pager.TabIndex = 9;
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(44, 192);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(69, 20);
      this.label8.TabIndex = 75;
      this.label8.Text = "Fax:";
      this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Fax
      // 
      this.Fax.Location = new System.Drawing.Point(116, 193);
      this.Fax.Name = "Fax";
      this.Fax.Size = new System.Drawing.Size(197, 20);
      this.Fax.TabIndex = 8;
      // 
      // label9
      // 
      this.label9.Location = new System.Drawing.Point(26, 140);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(84, 20);
      this.label9.TabIndex = 74;
      this.label9.Text = "Home phone:";
      this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Home_phone
      // 
      this.Home_phone.Location = new System.Drawing.Point(116, 141);
      this.Home_phone.Name = "Home_phone";
      this.Home_phone.Size = new System.Drawing.Size(333, 20);
      this.Home_phone.TabIndex = 6;
      // 
      // Cell_phone
      // 
      this.Cell_phone.Location = new System.Drawing.Point(116, 167);
      this.Cell_phone.Name = "Cell_phone";
      this.Cell_phone.Size = new System.Drawing.Size(333, 20);
      this.Cell_phone.TabIndex = 7;
      // 
      // label11
      // 
      this.label11.Location = new System.Drawing.Point(43, 62);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(69, 20);
      this.label11.TabIndex = 73;
      this.label11.Text = "Nickname:";
      this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // First_name
      // 
      this.First_name.Location = new System.Drawing.Point(116, 10);
      this.First_name.Name = "First_name";
      this.First_name.Size = new System.Drawing.Size(333, 20);
      this.First_name.TabIndex = 1;
      // 
      // label12
      // 
      this.label12.Location = new System.Drawing.Point(18, 114);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(92, 20);
      this.label12.TabIndex = 72;
      this.label12.Text = "Business phone:";
      this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label13
      // 
      this.label13.Location = new System.Drawing.Point(44, 166);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(69, 20);
      this.label13.TabIndex = 71;
      this.label13.Text = "Cell phone:";
      this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Family_name
      // 
      this.Family_name.Location = new System.Drawing.Point(116, 37);
      this.Family_name.Name = "Family_name";
      this.Family_name.Size = new System.Drawing.Size(333, 20);
      this.Family_name.TabIndex = 2;
      // 
      // label14
      // 
      this.label14.Location = new System.Drawing.Point(41, 88);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(69, 20);
      this.label14.TabIndex = 70;
      this.label14.Text = "E-mail:";
      this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label15
      // 
      this.label15.Location = new System.Drawing.Point(27, 36);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(85, 20);
      this.label15.TabIndex = 69;
      this.label15.Text = "Family Name:";
      this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label16
      // 
      this.label16.Location = new System.Drawing.Point(14, 9);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(98, 23);
      this.label16.TabIndex = 68;
      this.label16.Text = "First Name:";
      this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Business_phone
      // 
      this.Business_phone.Location = new System.Drawing.Point(116, 115);
      this.Business_phone.Name = "Business_phone";
      this.Business_phone.Size = new System.Drawing.Size(333, 20);
      this.Business_phone.TabIndex = 5;
      // 
      // Email
      // 
      this.Email.Location = new System.Drawing.Point(116, 89);
      this.Email.Name = "Email";
      this.Email.Size = new System.Drawing.Size(333, 20);
      this.Email.TabIndex = 4;
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(174)))), ((int)(((byte)(141)))), ((int)(((byte)(128)))));
      this.panel1.Controls.Add(this.label1);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel1.Location = new System.Drawing.Point(3, 282);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(714, 30);
      this.panel1.TabIndex = 32;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.ForeColor = System.Drawing.Color.White;
      this.label1.Location = new System.Drawing.Point(10, 7);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(114, 18);
      this.label1.TabIndex = 20;
      this.label1.Text = "Business data";
      // 
      // pBusiness
      // 
      this.pBusiness.BackColor = System.Drawing.SystemColors.Window;
      this.pBusiness.Controls.Add(this.Role_label);
      this.pBusiness.Controls.Add(this.Role);
      this.pBusiness.Controls.Add(this.Title_label);
      this.pBusiness.Controls.Add(this.Title);
      this.pBusiness.Controls.Add(this.Company_label);
      this.pBusiness.Controls.Add(this.Company);
      this.pBusiness.Controls.Add(this.country_company_label);
      this.pBusiness.Controls.Add(this.Pro_Country);
      this.pBusiness.Controls.Add(this.label10);
      this.pBusiness.Controls.Add(this.Pro_Post_Code);
      this.pBusiness.Controls.Add(this.region_state_company_label);
      this.pBusiness.Controls.Add(this.Pro_Region_State);
      this.pBusiness.Controls.Add(this.town_locality_company_label);
      this.pBusiness.Controls.Add(this.Adress_company_label);
      this.pBusiness.Controls.Add(this.Pro_Town);
      this.pBusiness.Controls.Add(this.Pro_Address2);
      this.pBusiness.Controls.Add(this.Pro_Address1);
      this.pBusiness.Dock = System.Windows.Forms.DockStyle.Top;
      this.pBusiness.Location = new System.Drawing.Point(3, 312);
      this.pBusiness.Name = "pBusiness";
      this.pBusiness.Size = new System.Drawing.Size(714, 190);
      this.pBusiness.TabIndex = 33;
      // 
      // Role_label
      // 
      this.Role_label.Location = new System.Drawing.Point(333, 8);
      this.Role_label.Name = "Role_label";
      this.Role_label.Size = new System.Drawing.Size(43, 23);
      this.Role_label.TabIndex = 61;
      this.Role_label.Text = "Role:";
      this.Role_label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Role
      // 
      this.Role.Location = new System.Drawing.Point(382, 10);
      this.Role.Name = "Role";
      this.Role.Size = new System.Drawing.Size(177, 20);
      this.Role.TabIndex = 21;
      // 
      // Title_label
      // 
      this.Title_label.Location = new System.Drawing.Point(41, 8);
      this.Title_label.Name = "Title_label";
      this.Title_label.Size = new System.Drawing.Size(69, 23);
      this.Title_label.TabIndex = 59;
      this.Title_label.Text = "Title:";
      this.Title_label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Title
      // 
      this.Title.Location = new System.Drawing.Point(116, 10);
      this.Title.Name = "Title";
      this.Title.Size = new System.Drawing.Size(197, 20);
      this.Title.TabIndex = 20;
      // 
      // Company_label
      // 
      this.Company_label.Location = new System.Drawing.Point(41, 33);
      this.Company_label.Name = "Company_label";
      this.Company_label.Size = new System.Drawing.Size(69, 23);
      this.Company_label.TabIndex = 57;
      this.Company_label.Text = "Company:";
      this.Company_label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Company
      // 
      this.Company.Location = new System.Drawing.Point(116, 35);
      this.Company.Name = "Company";
      this.Company.Size = new System.Drawing.Size(443, 20);
      this.Company.TabIndex = 22;
      // 
      // country_company_label
      // 
      this.country_company_label.Location = new System.Drawing.Point(22, 158);
      this.country_company_label.Name = "country_company_label";
      this.country_company_label.Size = new System.Drawing.Size(88, 23);
      this.country_company_label.TabIndex = 55;
      this.country_company_label.Text = "Country:";
      this.country_company_label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Pro_Country
      // 
      this.Pro_Country.Location = new System.Drawing.Point(116, 160);
      this.Pro_Country.Name = "Pro_Country";
      this.Pro_Country.Size = new System.Drawing.Size(443, 20);
      this.Pro_Country.TabIndex = 28;
      // 
      // label10
      // 
      this.label10.Location = new System.Drawing.Point(373, 133);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(76, 23);
      this.label10.TabIndex = 53;
      this.label10.Text = "Post. Code:";
      this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Pro_Post_Code
      // 
      this.Pro_Post_Code.Location = new System.Drawing.Point(455, 135);
      this.Pro_Post_Code.Name = "Pro_Post_Code";
      this.Pro_Post_Code.Size = new System.Drawing.Size(104, 20);
      this.Pro_Post_Code.TabIndex = 27;
      // 
      // region_state_company_label
      // 
      this.region_state_company_label.Location = new System.Drawing.Point(22, 133);
      this.region_state_company_label.Name = "region_state_company_label";
      this.region_state_company_label.Size = new System.Drawing.Size(88, 23);
      this.region_state_company_label.TabIndex = 51;
      this.region_state_company_label.Text = "Region / State:";
      this.region_state_company_label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Pro_Region_State
      // 
      this.Pro_Region_State.Location = new System.Drawing.Point(116, 135);
      this.Pro_Region_State.Name = "Pro_Region_State";
      this.Pro_Region_State.Size = new System.Drawing.Size(251, 20);
      this.Pro_Region_State.TabIndex = 26;
      // 
      // town_locality_company_label
      // 
      this.town_locality_company_label.Location = new System.Drawing.Point(22, 108);
      this.town_locality_company_label.Name = "town_locality_company_label";
      this.town_locality_company_label.Size = new System.Drawing.Size(88, 23);
      this.town_locality_company_label.TabIndex = 49;
      this.town_locality_company_label.Text = "Town / Locality:";
      this.town_locality_company_label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Adress_company_label
      // 
      this.Adress_company_label.Location = new System.Drawing.Point(41, 58);
      this.Adress_company_label.Name = "Adress_company_label";
      this.Adress_company_label.Size = new System.Drawing.Size(69, 23);
      this.Adress_company_label.TabIndex = 48;
      this.Adress_company_label.Text = "Address:";
      this.Adress_company_label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Pro_Town
      // 
      this.Pro_Town.Location = new System.Drawing.Point(116, 110);
      this.Pro_Town.Name = "Pro_Town";
      this.Pro_Town.Size = new System.Drawing.Size(443, 20);
      this.Pro_Town.TabIndex = 25;
      // 
      // Pro_Address2
      // 
      this.Pro_Address2.Location = new System.Drawing.Point(116, 85);
      this.Pro_Address2.Name = "Pro_Address2";
      this.Pro_Address2.Size = new System.Drawing.Size(443, 20);
      this.Pro_Address2.TabIndex = 24;
      // 
      // Pro_Address1
      // 
      this.Pro_Address1.Location = new System.Drawing.Point(116, 60);
      this.Pro_Address1.Name = "Pro_Address1";
      this.Pro_Address1.Size = new System.Drawing.Size(443, 20);
      this.Pro_Address1.TabIndex = 23;
      // 
      // panel3
      // 
      this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(174)))), ((int)(((byte)(141)))), ((int)(((byte)(128)))));
      this.panel3.Controls.Add(this.label2);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel3.Location = new System.Drawing.Point(3, 502);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(714, 30);
      this.panel3.TabIndex = 34;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.ForeColor = System.Drawing.Color.White;
      this.label2.Location = new System.Drawing.Point(10, 7);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(97, 18);
      this.label2.TabIndex = 20;
      this.label2.Text = "Private data";
      // 
      // pPrivate
      // 
      this.pPrivate.BackColor = System.Drawing.SystemColors.Window;
      this.pPrivate.Controls.Add(this.Birthday_label);
      this.pPrivate.Controls.Add(this.Birthday);
      this.pPrivate.Controls.Add(this.label6);
      this.pPrivate.Controls.Add(this.Country);
      this.pPrivate.Controls.Add(this.PotsCode_label);
      this.pPrivate.Controls.Add(this.Post_Code);
      this.pPrivate.Controls.Add(this.region_state_tab);
      this.pPrivate.Controls.Add(this.Region_State);
      this.pPrivate.Controls.Add(this.Town_locality_label);
      this.pPrivate.Controls.Add(this.label4);
      this.pPrivate.Controls.Add(this.Town);
      this.pPrivate.Controls.Add(this.Address2);
      this.pPrivate.Controls.Add(this.Address1);
      this.pPrivate.Dock = System.Windows.Forms.DockStyle.Top;
      this.pPrivate.Location = new System.Drawing.Point(3, 532);
      this.pPrivate.Name = "pPrivate";
      this.pPrivate.Size = new System.Drawing.Size(714, 164);
      this.pPrivate.TabIndex = 35;
      // 
      // Birthday_label
      // 
      this.Birthday_label.Location = new System.Drawing.Point(10, 133);
      this.Birthday_label.Name = "Birthday_label";
      this.Birthday_label.Size = new System.Drawing.Size(103, 23);
      this.Birthday_label.TabIndex = 40;
      this.Birthday_label.Text = "Birthday:";
      this.Birthday_label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Birthday
      // 
      this.Birthday.Location = new System.Drawing.Point(116, 135);
      this.Birthday.Name = "Birthday";
      this.Birthday.Size = new System.Drawing.Size(139, 20);
      this.Birthday.TabIndex = 36;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(25, 108);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(88, 23);
      this.label6.TabIndex = 38;
      this.label6.Text = "Country:";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Country
      // 
      this.Country.Location = new System.Drawing.Point(116, 110);
      this.Country.Name = "Country";
      this.Country.Size = new System.Drawing.Size(443, 20);
      this.Country.TabIndex = 35;
      // 
      // PotsCode_label
      // 
      this.PotsCode_label.Location = new System.Drawing.Point(373, 83);
      this.PotsCode_label.Name = "PotsCode_label";
      this.PotsCode_label.Size = new System.Drawing.Size(76, 23);
      this.PotsCode_label.TabIndex = 36;
      this.PotsCode_label.Text = "Post. Code:";
      this.PotsCode_label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Post_Code
      // 
      this.Post_Code.Location = new System.Drawing.Point(453, 85);
      this.Post_Code.Name = "Post_Code";
      this.Post_Code.Size = new System.Drawing.Size(106, 20);
      this.Post_Code.TabIndex = 34;
      // 
      // region_state_tab
      // 
      this.region_state_tab.Location = new System.Drawing.Point(25, 83);
      this.region_state_tab.Name = "region_state_tab";
      this.region_state_tab.Size = new System.Drawing.Size(88, 23);
      this.region_state_tab.TabIndex = 34;
      this.region_state_tab.Text = "Region / State:";
      this.region_state_tab.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Region_State
      // 
      this.Region_State.Location = new System.Drawing.Point(116, 85);
      this.Region_State.Name = "Region_State";
      this.Region_State.Size = new System.Drawing.Size(251, 20);
      this.Region_State.TabIndex = 33;
      // 
      // Town_locality_label
      // 
      this.Town_locality_label.Location = new System.Drawing.Point(25, 58);
      this.Town_locality_label.Name = "Town_locality_label";
      this.Town_locality_label.Size = new System.Drawing.Size(88, 23);
      this.Town_locality_label.TabIndex = 32;
      this.Town_locality_label.Text = "Town / Locality:";
      this.Town_locality_label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(44, 8);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(69, 23);
      this.label4.TabIndex = 31;
      this.label4.Text = "Address:";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // Town
      // 
      this.Town.Location = new System.Drawing.Point(116, 60);
      this.Town.Name = "Town";
      this.Town.Size = new System.Drawing.Size(443, 20);
      this.Town.TabIndex = 32;
      // 
      // Address2
      // 
      this.Address2.Location = new System.Drawing.Point(116, 35);
      this.Address2.Name = "Address2";
      this.Address2.Size = new System.Drawing.Size(443, 20);
      this.Address2.TabIndex = 31;
      // 
      // Address1
      // 
      this.Address1.Location = new System.Drawing.Point(116, 10);
      this.Address1.Name = "Address1";
      this.Address1.Size = new System.Drawing.Size(443, 20);
      this.Address1.TabIndex = 28;
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.FileName = "openFileDialog1";
      // 
      // RtdVCardControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.pPrivate);
      this.Controls.Add(this.panel3);
      this.Controls.Add(this.pBusiness);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.pContact);
      this.Controls.Add(this.pTitle);
      this.Name = "RtdVCardControl";
      this.Size = new System.Drawing.Size(720, 728);
      this.pTitle.ResumeLayout(false);
      this.pTitle.PerformLayout();
      this.pContact.ResumeLayout(false);
      this.pContact.PerformLayout();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.pBusiness.ResumeLayout(false);
      this.pBusiness.PerformLayout();
      this.panel3.ResumeLayout(false);
      this.panel3.PerformLayout();
      this.pPrivate.ResumeLayout(false);
      this.pPrivate.PerformLayout();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Label lPSize;
    private System.Windows.Forms.Button bRemovePicture;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private System.Windows.Forms.Label label16;
    private System.Windows.Forms.Label label15;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.FlowLayoutPanel flpPicture;
    private System.Windows.Forms.Button bChangePicture;
    private System.Windows.Forms.Panel pPrivate;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Panel pBusiness;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel pContact;
    private System.Windows.Forms.Label label19;
    private System.Windows.Forms.Panel pTitle;
    private System.Windows.Forms.TextBox Pro_Address1;
    private System.Windows.Forms.TextBox Pro_Address2;
    private System.Windows.Forms.TextBox Pro_Town;
    private System.Windows.Forms.Label Adress_company_label;
    private System.Windows.Forms.Label town_locality_company_label;
    private System.Windows.Forms.TextBox Pro_Region_State;
    private System.Windows.Forms.Label region_state_company_label;
    private System.Windows.Forms.TextBox Pro_Post_Code;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.TextBox Pro_Country;
    private System.Windows.Forms.Label country_company_label;
    private System.Windows.Forms.TextBox Company;
    private System.Windows.Forms.Label Company_label;
    private System.Windows.Forms.TextBox Title;
    private System.Windows.Forms.Label Title_label;
    private System.Windows.Forms.TextBox Role;
    private System.Windows.Forms.Label Role_label;
    private System.Windows.Forms.TextBox Address1;
    private System.Windows.Forms.TextBox Address2;
    private System.Windows.Forms.TextBox Town;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label Town_locality_label;
    private System.Windows.Forms.TextBox Region_State;
    private System.Windows.Forms.Label region_state_tab;
    private System.Windows.Forms.TextBox Post_Code;
    private System.Windows.Forms.Label PotsCode_label;
    private System.Windows.Forms.TextBox Country;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.TextBox Birthday;
    private System.Windows.Forms.Label Birthday_label;
    private System.Windows.Forms.TextBox Email;
    private System.Windows.Forms.TextBox Business_phone;
    private System.Windows.Forms.TextBox Family_name;
    private System.Windows.Forms.TextBox First_name;
    private System.Windows.Forms.TextBox Cell_phone;
    private System.Windows.Forms.TextBox Home_phone;
    private System.Windows.Forms.TextBox Fax;
    private System.Windows.Forms.TextBox Pager;
    private System.Windows.Forms.TextBox Email_alternative;
    private System.Windows.Forms.TextBox Nickname;
  }
}
