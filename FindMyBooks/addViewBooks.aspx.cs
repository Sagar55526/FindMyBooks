﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FindMyBooks
{
    public partial class addViewBooks : System.Web.UI.Page
    {
        readonly string strcon = ConfigurationManager.ConnectionStrings["con"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!this.IsPostBack)
            {



                //conecting yearID data table to drop down
                using (SqlConnection con = new SqlConnection(strcon))
                {
                    using (SqlCommand cmd = new SqlCommand("select AcademicYear from YearTable", con))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        ddlAcademicYear.DataSource = dt;
                        ddlAcademicYear.DataValueField = "AcademicYear";
                        ddlAcademicYear.DataBind();
                    }
                }
                //conecting tbl_comment_bool data table to drop down
                using (SqlConnection con = new SqlConnection(strcon))
                {
                    using (SqlCommand cmd = new SqlCommand("select comment from tbl_comment_book", con))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        ddlBookComment.DataSource = dt;
                        ddlBookComment.DataValueField = "comment";
                        ddlBookComment.DataBind();
                    }
                }
                //conecting tbl_dept_name data table to drop down
                using (SqlConnection con = new SqlConnection(strcon))
                {
                    using (SqlCommand cmd = new SqlCommand("select deptName from tbl_dept_name", con))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        ddlDeptName.DataSource = dt;
                        ddlDeptName.DataValueField = "deptName";
                        ddlDeptName.DataBind();
                    }
                }
                ddlAcademicYear.Items.Insert(0, new ListItem("--Select academic year--", "0"));
                ddlBookComment.Items.Insert(0, new ListItem("--Give comment on book--", "0"));
                ddlDeptName.Items.Insert(0, new ListItem("--Select department--", "0"));
            }

        }

        //for radio buttons controls.
        protected void radiobtnSingleBook_CheckedChanged(object sender, EventArgs e)
        {
            txtTotalBooks.Text = "1";
            txtTotalBooks.ReadOnly = true; 
        }

        protected void radbtnManyBook_CheckedChanged(object sender, EventArgs e)
        {
            txtTotalBooks.Text = "";
            txtTotalBooks.ReadOnly = false;
            txtCost.ReadOnly = true;

            string[] bookCosts = txtTotalBooks.Text.Split(',');
            foreach (string bookCost in bookCosts)
            {
                double cost;
                if (double.TryParse(bookCost.Trim(), out cost))
                {
                    txtCost.Text += bookCost.Trim() + ", ";
                }
            }
            if (!string.IsNullOrEmpty(txtCost.Text))
            {
                txtCost.Text = txtCost.Text.TrimEnd(' ', ',');
            }
        }


        //event  handler for add book btn
        protected void btnAddBook_Click(object sender, EventArgs e)
        {
            if (Session["username"] != null)
            {
                string username = Session["username"].ToString();

                int stdID = GetStdID(username);

                string selectedYear = ddlAcademicYear.SelectedItem.Text;
                int yearIdResult = getYearID(selectedYear);

                string selectedDept = ddlDeptName.SelectedItem.Text;
                int deptIdResult = getDeptNameID(selectedDept); 
                
                string selectedComment = ddlBookComment.SelectedItem.Text;
                int commentResult = getCommentID(selectedComment);

                if (stdID > 0)
                {
                    using (SqlConnection con = new SqlConnection(strcon))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }

                        SqlCommand insertCmd = new SqlCommand("INSERT INTO tbl_book_add(deptName, publicationName, subjectName, bookComment, bookCost, status, stdID, academicYearID)" +
                            "VALUES(@deptName, @publicationName, @subjectName, @bookComment, @bookCost, @status, @stdID, @academicYearID)", con);
                        {
                            //insertCmd.Parameters.AddWithValue("@deptName", ddlDeptName.SelectedItem != null ? ddlDeptName.SelectedItem.Text : "");
                            insertCmd.Parameters.AddWithValue("@publicationName", txtPublication.Text.Trim());
                            insertCmd.Parameters.AddWithValue("@bookCost", txtCost.Text.Trim());
                            insertCmd.Parameters.AddWithValue("@status", txtStatus.Text.Trim()); 
                            insertCmd.Parameters.AddWithValue("@subjectName", txtSubjectName.Text.Trim());
                            insertCmd.Parameters.AddWithValue("@stdID", stdID);
                            insertCmd.Parameters.AddWithValue("@academicYearID", yearIdResult);
                            insertCmd.Parameters.AddWithValue("@deptName", deptIdResult); 
                            insertCmd.Parameters.AddWithValue("@bookComment", commentResult);

                            insertCmd.ExecuteNonQuery();
                            con.Close();
                            Response.Write("<script>alert('Book added successfully.')</script>");
                        }
                    }
                }
                else
                {
                    
                    Response.Write("<script>alert('Failed to fetch user information.!!!')</script>");
                }
            }
            else
            {
                
                Response.Write("<script>alert('User is not logged in.')</script>");
            }
        }



        //userLogin defined functions


        private int GetStdID(string username)
        {
            int stdID = 0;

            using (SqlConnection con = new SqlConnection(strcon))
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                SqlCommand cmd = new SqlCommand("select stdID from tbl_user_master where stdUserName = @username", con);
                cmd.Parameters.AddWithValue("@username", username);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    stdID = Convert.ToInt32(reader["stdID"]);
                }

                reader.Close();
                con.Close();
            }

            return stdID;
        }

        private int getYearID(string selectedYear)
        {
            int yearIdResult = 0;

            using (SqlConnection con = new SqlConnection(strcon))
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                SqlCommand cmd = new SqlCommand("SELECT yearID FROM YearTable WHERE AcademicYear = @academicYear", con);
                cmd.Parameters.AddWithValue("@academicYear", selectedYear);

                
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    yearIdResult = Convert.ToInt32(result);
                }
            }

            return yearIdResult;
        }

        private int getDeptNameID(string selectedDept)
        {
            int deptIdResult = 0;

            using (SqlConnection con = new SqlConnection(strcon))
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                SqlCommand cmd = new SqlCommand("SELECT deptID FROM tbl_dept_name WHERE deptName = @deptName", con);
                cmd.Parameters.AddWithValue("@deptName", selectedDept);


                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    deptIdResult = Convert.ToInt32(result);
                }
            }

            return deptIdResult;
        }

        private int getCommentID(string selectedComment)
        {
            int commentResult = 0;

            using (SqlConnection con = new SqlConnection(strcon))
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                SqlCommand cmd = new SqlCommand("SELECT commentID FROM tbl_comment_book WHERE comment = @comment", con);
                cmd.Parameters.AddWithValue("@comment", selectedComment);


                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    commentResult = Convert.ToInt32(result);
                }
            }

            return commentResult;
        }


    }
}