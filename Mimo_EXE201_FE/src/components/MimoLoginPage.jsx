import React, { useState } from 'react';
import './MimoRegisterPage.css';

const MimoLoginPage = ({ onLoginSuccess, onRegisterClick, onHomeClick }) => {
  const [showPassword, setShowPassword] = useState(false);

  const handleSubmit = (e) => {
    e.preventDefault();
    // Simulate login
    onLoginSuccess();
  };

  return (
    <div className="mimo-register-page">
      {/* Header */}
      <header className="register-header">
        <a href="#" className="register-logo" onClick={(e) => { e.preventDefault(); if (onHomeClick) onHomeClick(); }}>
          <div className="register-logo-icon">🤖</div>
          mimo
        </a>
        <div className="register-login-link">
          Chưa có tài khoản? <a href="#" onClick={(e) => { e.preventDefault(); onRegisterClick(); }}>Đăng ký</a>
        </div>
      </header>

      {/* Main Content */}
      <main className="register-main">
        
        {/* Left Column */}
        <div className="register-left">
          <div className="register-subtitle">CHÀO MỪNG TRỞ LẠI</div>
          <h1>Tiếp tục hành trình học kỹ năng sống của bạn!</h1>
          <p className="register-left-desc">
            Đăng nhập vào Mimo để tiếp tục học và rèn luyện các kỹ năng an toàn qua các tình huống thực tế.
          </p>

          <div className="register-benefits">
            <div className="register-benefit-item">
              <div className="benefit-icon-wrapper bg-blue">🔥</div>
              <div className="benefit-text">
                <h4>Giữ vững chuỗi ngày học tập</h4>
                <p>Tiếp tục nhận XP và huy hiệu</p>
              </div>
            </div>
            <div className="register-benefit-item">
              <div className="benefit-icon-wrapper bg-green">🛡️</div>
              <div className="benefit-text">
                <h4>Nâng cao kỹ năng an toàn</h4>
                <p>Khám phá thêm nhiều tình huống mới</p>
              </div>
            </div>
          </div>

          <div className="register-mascot-area">
            <div className="mascot-large">🤖</div>
            <div className="mascot-quote">
              "Mimo rất vui khi gặp lại bạn!"
            </div>
            <div style={{ position: 'absolute', right: '0', bottom: '20px', fontFamily: 'Comic Sans MS', color: '#3b82f6', transform: 'rotate(-5deg)' }}>
              Cố lên<br/>nhé!
            </div>
          </div>
        </div>

        {/* Right Column (Form) */}
        <div className="register-right">
          <div className="register-card">
            <h2>Đăng nhập Mimo</h2>
            <p>Nhập email và mật khẩu để vào tài khoản của bạn</p>

            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Email</label>
                <div className="input-wrapper">
                  <span className="input-icon">✉️</span>
                  <input type="email" placeholder="Nhập email của bạn" required />
                </div>
              </div>

              <div className="form-group">
                <label>Mật khẩu</label>
                <div className="input-wrapper">
                  <span className="input-icon">🔒</span>
                  <input 
                    type={showPassword ? "text" : "password"} 
                    placeholder="Nhập mật khẩu" 
                    required 
                  />
                  <button 
                    type="button" 
                    className="input-icon-right"
                    onClick={() => setShowPassword(!showPassword)}
                  >
                    {showPassword ? "🙈" : "👁️"}
                  </button>
                </div>
                <div style={{ textAlign: 'right', marginTop: '8px' }}>
                  <a href="#" style={{ fontSize: '12px', color: '#3b82f6', textDecoration: 'none' }}>Quên mật khẩu?</a>
                </div>
              </div>

              <button type="submit" className="btn-register-submit">
                Đăng nhập →
              </button>
            </form>

            <div className="divider">Hoặc đăng nhập bằng</div>

            <div className="social-login-group">
              <button className="btn-social">
                <span>G</span> Google
              </button>
              <button className="btn-social">
                <span style={{ color: '#1877f2' }}>f</span> Facebook
              </button>
              <button className="btn-social">
                <span></span> Apple
              </button>
            </div>
          </div>
        </div>
      </main>
      
      <div className="bg-shape-bottom"></div>
    </div>
  );
};

export default MimoLoginPage;
