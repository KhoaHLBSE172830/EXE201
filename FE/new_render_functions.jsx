  const renderLeaderboardContent = () => (
    <>
      <section className="leaderboard-hero">
        <div className="lb-hero-content">
          <h1>🏆 Bảng xếp hạng</h1>
          <p>Cùng nhau học tập, cùng nhau tạo nên một môi trường an toàn hơn!</p>
        </div>
        <div className="lb-hero-quote">
           Nỗ lực hôm nay<br/>là một phiên bản tốt hơn<br/>của ngày mai! ✨
        </div>
        <div className="lb-hero-mascot">
           <div style={{fontSize: '110px'}}>🤖🏆</div>
        </div>
      </section>

      <div className="lb-filters">
        <div className="filter-chips">
          <button className="chip active">Tất cả</button>
          <button className="chip">Bạn bè</button>
          <button className="chip">Trong trường</button>
          <button className="chip">Theo lớp</button>
        </div>
        <div className="filter-dropdowns">
          <button className="btn-sort">🗓️ Tuần này ▾</button>
        </div>
      </div>

      <div className="podium-container">
        {/* Rank 2 */}
        <div className="podium-item rank-2">
           <div className="podium-avatar">
             <div className="crown">🥈</div>
             <div style={{fontSize: '40px'}}>👧🏻</div>
             <div className="rank-badge">2</div>
           </div>
           <h4>Trần Phương Anh</h4>
           <p>THPT FPT</p>
           <strong>2.980 XP</strong>
           <div className="podium-base blue"></div>
        </div>
        {/* Rank 1 */}
        <div className="podium-item rank-1">
           <div className="podium-avatar">
             <div className="crown">👑</div>
             <div style={{fontSize: '50px'}}>👦🏻</div>
             <div className="rank-badge">1</div>
           </div>
           <h4>Nguyễn Minh Quân</h4>
           <p>THPT FPT</p>
           <strong>3.250 XP</strong>
           <div className="podium-base gold"></div>
        </div>
        {/* Rank 3 */}
        <div className="podium-item rank-3">
           <div className="podium-avatar">
             <div className="crown">🥉</div>
             <div style={{fontSize: '40px'}}>👦🏽</div>
             <div className="rank-badge">3</div>
           </div>
           <h4>Lê Hoàng Bảo</h4>
           <p>THPT FPT</p>
           <strong>2.760 XP</strong>
           <div className="podium-base orange"></div>
        </div>
      </div>

      <div className="lb-table">
        <div className="lb-row header">
          <div className="lb-col center" style={{width: '40px'}}>#</div>
          <div className="lb-col flex-2">Người dùng</div>
          <div className="lb-col flex-2">Trường / Lớp</div>
          <div className="lb-col center">Cấp độ</div>
          <div className="lb-col center">XP</div>
          <div className="lb-col center">Chuỗi ngày</div>
          <div className="lb-col center">Huy hiệu</div>
        </div>
        
        <div className="lb-row">
          <div className="lb-col center text-gray" style={{width: '40px'}}>4</div>
          <div className="lb-col flex-2 user-col"><div className="lb-avatar">👧🏻</div> Phạm Ngọc Linh</div>
          <div className="lb-col flex-2 text-gray">THPT FPT - 12A1</div>
          <div className="lb-col center text-orange font-bold">👑 6</div>
          <div className="lb-col center font-bold">2.540</div>
          <div className="lb-col center text-red font-bold">🔥 12</div>
          <div className="lb-col center badge-col">
            <span className="mini-badge green">🛡️</span>
            <span className="mini-badge red">🔥</span>
            <span className="mini-badge yellow">⭐</span>
            <span className="mini-badge purple">🛡️</span>
          </div>
        </div>
        <div className="lb-row">
          <div className="lb-col center text-gray" style={{width: '40px'}}>5</div>
          <div className="lb-col flex-2 user-col"><div className="lb-avatar">👦🏻</div> Vũ Đức Huy</div>
          <div className="lb-col flex-2 text-gray">THPT FPT - 12A1</div>
          <div className="lb-col center text-orange font-bold">👑 5</div>
          <div className="lb-col center font-bold">2.380</div>
          <div className="lb-col center text-red font-bold">🔥 9</div>
          <div className="lb-col center badge-col">
             <span className="mini-badge blue">🚗</span>
             <span className="mini-badge red">🔥</span>
             <span className="mini-badge green">➕</span>
             <span className="mini-badge purple">🛡️</span>
          </div>
        </div>
        <div className="lb-row active-user">
          <div className="lb-col center text-blue font-bold" style={{width: '40px'}}>6</div>
          <div className="lb-col flex-2 user-col"><div className="lb-avatar">👦🏻</div> <strong>Hồ Xuân Anh Nam (Bạn)</strong></div>
          <div className="lb-col flex-2 text-blue font-bold">THPT FPT - 12A1</div>
          <div className="lb-col center text-orange font-bold">👑 5</div>
          <div className="lb-col center font-bold text-blue">2.150</div>
          <div className="lb-col center text-red font-bold">🔥 7</div>
          <div className="lb-col center badge-col">
             <span className="mini-badge blue">🚗</span>
             <span className="mini-badge green">➕</span>
             <span className="mini-badge purple">🛡️</span>
          </div>
        </div>
        <div className="lb-row">
          <div className="lb-col center text-gray" style={{width: '40px'}}>7</div>
          <div className="lb-col flex-2 user-col"><div className="lb-avatar">👧🏻</div> Đặng Thu Hà</div>
          <div className="lb-col flex-2 text-gray">THPT FPT - 12A2</div>
          <div className="lb-col center text-orange font-bold">👑 5</div>
          <div className="lb-col center font-bold">2.010</div>
          <div className="lb-col center text-red font-bold">🔥 6</div>
          <div className="lb-col center badge-col">
             <span className="mini-badge blue">🚗</span>
             <span className="mini-badge red">🔥</span>
             <span className="mini-badge purple">🛡️</span>
          </div>
        </div>
        <div className="lb-row">
          <div className="lb-col center text-gray" style={{width: '40px'}}>8</div>
          <div className="lb-col flex-2 user-col"><div className="lb-avatar">👦🏻</div> Nguyễn Gia Bảo</div>
          <div className="lb-col flex-2 text-gray">THPT FPT - 12A3</div>
          <div className="lb-col center text-orange font-bold">👑 4</div>
          <div className="lb-col center font-bold">1.960</div>
          <div className="lb-col center text-red font-bold">🔥 5</div>
          <div className="lb-col center badge-col">
             <span className="mini-badge green">➕</span>
             <span className="mini-badge yellow">⭐</span>
          </div>
        </div>
        <div className="lb-row">
          <div className="lb-col center text-gray" style={{width: '40px'}}>9</div>
          <div className="lb-col flex-2 user-col"><div className="lb-avatar">👦🏻</div> Trần Minh Khang</div>
          <div className="lb-col flex-2 text-gray">THPT FPT - 12A1</div>
          <div className="lb-col center text-orange font-bold">👑 4</div>
          <div className="lb-col center font-bold">1.850</div>
          <div className="lb-col center text-red font-bold">🔥 4</div>
          <div className="lb-col center badge-col">
             <span className="mini-badge blue">🚗</span>
             <span className="mini-badge red">🔥</span>
             <span className="mini-badge yellow">⭐</span>
             <span className="mini-badge purple">🛡️</span>
          </div>
        </div>
        <div className="lb-row">
          <div className="lb-col center text-gray" style={{width: '40px'}}>10</div>
          <div className="lb-col flex-2 user-col"><div className="lb-avatar">👧🏻</div> Lê Thảo Vy</div>
          <div className="lb-col flex-2 text-gray">THPT FPT - 12A4</div>
          <div className="lb-col center text-orange font-bold">👑 4</div>
          <div className="lb-col center font-bold">1.720</div>
          <div className="lb-col center text-red font-bold">🔥 4</div>
          <div className="lb-col center badge-col">
             <span className="mini-badge green">➕</span>
             <span className="mini-badge red">🔥</span>
             <span className="mini-badge blue">🌀</span>
          </div>
        </div>
        
        <button className="btn-view-more-lb">Xem thêm ▾</button>
      </div>
    </>
  );

  const renderLeaderboardRightPanel = () => (
    <>
      <div className="mimo-panel-card text-center" style={{paddingBottom: '24px'}}>
         <div className="panel-header" style={{borderBottom: 'none', justifyContent: 'center'}}>
           <h3 style={{color: '#1e3a8a'}}>📊 Hạng của bạn</h3>
         </div>
         <div className="user-rank-large">
           <div className="avatar-xl">👦</div>
           <h2 className="text-blue">#6</h2>
           <p className="font-bold text-gray-800">Trong trường</p>
           <p className="text-gray text-sm">THPT FPT</p>
         </div>
         <div className="lb-progress-wrap mt-15">
           <div className="progress-track" style={{background: '#e0f2fe'}}><div className="progress-fill" style={{width: '85%', background: '#3b82f6'}}></div></div>
           <div className="flex-between mt-5 text-xs">
              <strong className="text-blue">2.150 XP</strong>
              <span className="text-gray">Còn 350 XP để lên Cấp 6</span>
           </div>
         </div>
      </div>

      <div className="mimo-panel-card">
        <div className="panel-header">
          <h3 style={{color: '#dc2626'}}>🎯 Bảng xếp hạng theo kỹ năng</h3>
        </div>
        <div className="skill-rank-list">
           <div className="skill-rank-item">
             <div className="icon bg-blue-light text-blue">🚗</div>
             <span>An toàn giao thông</span>
             <strong className="ml-auto">#4 ›</strong>
           </div>
           <div className="skill-rank-item">
             <div className="icon bg-orange-light text-orange">🔥</div>
             <span>Hỏa hoạn và Thoát hiểm</span>
             <strong className="ml-auto">#7 ›</strong>
           </div>
           <div className="skill-rank-item">
             <div className="icon bg-green-light text-green">➕</div>
             <span>Sơ cứu</span>
             <strong className="ml-auto">#6 ›</strong>
           </div>
           <div className="skill-rank-item">
             <div className="icon bg-purple-light text-purple">🛡️</div>
             <span>Bạo lực gia đình/học đường</span>
             <strong className="ml-auto">#8 ›</strong>
           </div>
        </div>
      </div>
      
      <div className="feedback-card bg-blue-light flex-col text-center promo-banner">
         <p className="text-blue font-bold text-sm mb-10">Không quan trọng bạn đang ở vị trí nào, quan trọng là bạn luôn tiến lên mỗi ngày!</p>
         <div style={{fontSize: '60px', marginBottom: '10px'}}>🤖🚩</div>
         <button className="btn-blue-gradient w-full">Tiếp tục học ngay →</button>
      </div>
    </>
  );

  const renderProfileContent = () => (
    <div className="profile-tab-wrapper">
      <section className="profile-hero-banner">
         <div className="profile-hero-content">
            <div className="profile-avatar-xl">
               👦
               <button className="btn-camera">📷</button>
            </div>
            <div className="profile-hero-info">
               <h1>Nam <button className="btn-edit-icon">✏️</button></h1>
               <p className="quote">Học an toàn hôm nay, cuộc sống tốt đẹp hơn ngày mai! 🌟</p>
               <div className="tags-row">
                 <span className="tag-blue">Học sinh THPT</span>
                 <span className="tag-orange">Cấp 5</span>
                 <span className="tag-green">Người bảo vệ</span>
                 <button className="btn-edit-icon sm">✏️</button>
               </div>
            </div>
         </div>
         <div className="profile-hero-mascot">
            <div className="mascot-bubble" style={{whiteSpace: 'nowrap'}}>Cố lên Nam!<br/>Bạn đang làm rất tốt!</div>
            <div style={{fontSize: '90px'}}>🤖✨</div>
         </div>
      </section>

      <div className="profile-grid">
         {/* Column 1 */}
         <div className="profile-col">
            <div className="mimo-panel-card" style={{height: '100%'}}>
               <div className="panel-header">
                 <h3 className="text-blue flex-align-center gap-10">👤 Thông tin cá nhân</h3>
                 <button className="btn-text-blue flex-align-center gap-5">✏️ Chỉnh sửa</button>
               </div>
               <div className="info-list">
                 <div className="info-row"><div className="info-label">👤 Họ và tên</div><div className="info-value">Hồ Xuân Anh Nam</div></div>
                 <div className="info-row"><div className="info-label">📅 Ngày sinh</div><div className="info-value">20/04/2004</div></div>
                 <div className="info-row"><div className="info-label">⚧️ Giới tính</div><div className="info-value">Nam</div></div>
                 <div className="info-row"><div className="info-label">🎓 Trường học</div><div className="info-value">Đại học FPT</div></div>
                 <div className="info-row"><div className="info-label">✉️ Email</div><div className="info-value">nam@example.com</div></div>
                 <div className="info-row"><div className="info-label">📞 Số điện thoại</div><div className="info-value">0123 456 789</div></div>
                 <div className="info-row"><div className="info-label">📍 Địa chỉ</div><div className="info-value">Việt Nam</div></div>
               </div>
            </div>
         </div>

         {/* Column 2 */}
         <div className="profile-col flex-col gap-24">
            <div className="mimo-panel-card">
               <div className="panel-header">
                 <h3 className="text-green flex-align-center gap-10"><span style={{fontSize: '20px'}}>❝</span> Giới thiệu bản thân</h3>
                 <button className="btn-text-blue flex-align-center gap-5">✏️ Chỉnh sửa</button>
               </div>
               <p className="bio-text">
                 Mình muốn học thêm nhiều kỹ năng để bảo vệ bản thân, gia đình và những người xung quanh. Cùng Mimo tạo nên một môi trường an toàn hơn nhé! 💙
               </p>
            </div>

            <div className="mimo-panel-card">
               <div className="panel-header">
                 <h3 className="text-red flex-align-center gap-10">❤️ Sở thích</h3>
                 <button className="btn-text-blue flex-align-center gap-5">✏️ Chỉnh sửa</button>
               </div>
               <div className="hobby-chips">
                  <span className="chip-hobby">🎾 Chơi tennis</span>
                  <span className="chip-hobby">📚 Học kỹ năng sống</span>
                  <span className="chip-hobby">🎮 Công nghệ</span>
                  <span className="chip-hobby">🎵 Nghe nhạc</span>
                  <span className="chip-hobby">✈️ Du lịch</span>
                  <button className="btn-add-hobby">+ Thêm sở thích</button>
               </div>
            </div>
         </div>
      </div>

      <div className="profile-grid mt-24">
         {/* Row 2 - Col 1 */}
         <div className="profile-col">
            <div className="mimo-panel-card" style={{height: '100%'}}>
               <div className="panel-header">
                 <h3 className="text-purple flex-align-center gap-10"><span style={{fontSize: '20px'}}>📊</span> Thống kê nhanh</h3>
               </div>
               <div className="stats-grid-2x2">
                 <div className="stat-box bg-orange-light">
                   <div className="icon text-orange">⭐</div>
                   <div className="val text-orange">1.250</div>
                   <div className="lbl text-orange">Điểm XP</div>
                 </div>
                 <div className="stat-box bg-blue-light">
                   <div className="icon text-blue">👑</div>
                   <div className="val text-blue">Cấp 5</div>
                   <div className="lbl text-blue">Người bảo vệ</div>
                 </div>
                 <div className="stat-box bg-red-light">
                   <div className="icon text-red">🔥</div>
                   <div className="val text-red">7 ngày</div>
                   <div className="lbl text-red">Chuỗi học liên tục</div>
                 </div>
                 <div className="stat-box bg-green-light">
                   <div className="icon text-green">📖</div>
                   <div className="val text-green">18</div>
                   <div className="lbl text-green">Bài học đã hoàn thành</div>
                 </div>
               </div>
            </div>
         </div>

         {/* Row 2 - Col 2 */}
         <div className="profile-col">
            <div className="mimo-panel-card" style={{height: '100%'}}>
               <div className="panel-header">
                 <h3 className="text-red flex-align-center gap-10"><span style={{fontSize: '20px'}}>🎯</span> Mục tiêu của tôi</h3>
                 <button className="btn-text-blue flex-align-center gap-5">✏️ Chỉnh sửa</button>
               </div>
               <div className="goal-list-profile">
                 <div className="goal-item-profile">
                    <div className="num">1</div>
                    <div className="txt">Hoàn thành tất cả các chủ đề kỹ năng</div>
                 </div>
                 <div className="goal-item-profile">
                    <div className="num">2</div>
                    <div className="txt">Duy trì chuỗi học liên tục 30 ngày</div>
                 </div>
                 <div className="goal-item-profile">
                    <div className="num">3</div>
                    <div className="txt">Đạt Cấp 10 - Anh hùng Mimo</div>
                 </div>
                 <button className="btn-add-goal-profile">+ Thêm mục tiêu mới</button>
               </div>
            </div>
         </div>
      </div>
      
      <div className="profile-footer-banner mt-24">
         <div style={{fontSize: '40px'}}>🤖🎓</div>
         <span style={{fontSize: '16px', fontWeight: 'bold', color: '#1e3a8a'}}>Mỗi bước tiến nhỏ hôm nay sẽ tạo nên một phiên bản tốt hơn của bạn ngày mai! 🍃</span>
      </div>
    </div>
  );

  const renderAchievementsContent = () => (
    <>
      <section className="achieve-hero">
        <div className="achieve-hero-content">
          <h1>Thành tích của bạn</h1>
          <p>Mỗi kỹ năng nhỏ hôm nay đều tạo nên sự khác biệt lớn ngày mai!</p>
        </div>
        <div className="achieve-hero-mascot">
           <div className="mascot-bubble">Bạn đang làm<br/>tuyệt vời lắm!</div>
           <div className="mascot-bubble right">Cố lên<br/>Nam!</div>
           <div style={{fontSize: '110px'}}>🤖🌟</div>
        </div>
      </section>

      <section className="achieve-stats-row">
         <div className="achieve-stat-box">
           <div className="icon text-orange bg-orange-light">⭐</div>
           <div>
             <h3>3.250 XP</h3>
             <p>Tổng điểm kinh nghiệm</p>
           </div>
         </div>
         <div className="achieve-stat-box">
           <div className="icon text-blue bg-blue-light">👑</div>
           <div>
             <h3>Cấp 6</h3>
             <p>Cấp hiện tại</p>
           </div>
         </div>
         <div className="achieve-stat-box">
           <div className="icon text-red bg-red-light">🔥</div>
           <div>
             <h3>12 ngày</h3>
             <p>Chuỗi ngày học liên tục</p>
           </div>
         </div>
         <div className="achieve-stat-box">
           <div className="icon text-blue bg-blue-light">📖</div>
           <div>
             <h3>18 bài</h3>
             <p>Bài học đã hoàn thành</p>
           </div>
         </div>
         <div className="achieve-stat-box">
           <div className="icon text-green bg-green-light">🚩</div>
           <div>
             <h3>7 tình huống</h3>
             <p>Tình huống mô phỏng</p>
           </div>
         </div>
      </section>

      <section className="mimo-panel-card p-24">
         <div className="panel-header">
           <h2 className="text-green flex-align-center gap-10"><span style={{fontSize:'24px'}}>⚙️</span> Huy hiệu đã đạt được</h2>
           <a href="#" className="text-blue font-bold">Xem tất cả →</a>
         </div>
         <div className="badge-grid mt-15">
            <div className="badge-card">
              <div className="badge-icon-lg fire">🔥</div>
              <h4>Kiên trì 7 ngày</h4>
              <p>Học liên tục 7 ngày</p>
              <div className="date">Đạt ngày 10/04/2024</div>
            </div>
            <div className="badge-card">
              <div className="badge-icon-lg aid">🚑</div>
              <h4>Người cứu hộ nhí</h4>
              <p>Hoàn thành 5 bài sơ cứu</p>
              <div className="date">Đạt ngày 08/04/2024</div>
            </div>
            <div className="badge-card">
              <div className="badge-icon-lg traffic">🚦</div>
              <h4>Người tham gia an toàn giao thông</h4>
              <p>Hoàn thành 10 bài học</p>
              <div className="date">Đạt ngày 05/04/2024</div>
            </div>
            <div className="badge-card">
              <div className="badge-icon-lg fire-esc">🔥</div>
              <h4>Thoát hiểm thông minh</h4>
              <p>Hoàn thành 5 tình huống thoát hiểm</p>
              <div className="date">Đạt ngày 02/04/2024</div>
            </div>
            <div className="badge-card">
              <div className="badge-icon-lg heart">❤️</div>
              <h4>Bạn tốt biết lắng nghe</h4>
              <p>Hoàn thành 5 bài học về bạo lực gia đình/học đường</p>
              <div className="date">Đạt ngày 30/03/2024</div>
            </div>
            <div className="badge-card">
              <div className="badge-icon-lg flash">⚡</div>
              <h4>Phản ứng nhanh</h4>
              <p>Hoàn thành 3 tình huống thử thách thời gian</p>
              <div className="date">Đạt ngày 28/03/2024</div>
            </div>
            <div className="badge-card locked">
              <div className="badge-icon-lg lock">🔒</div>
              <h4>Nhà thám hiểm Mimo</h4>
              <p>Hoàn thành 20 bài học</p>
              <div className="progress-track mt-10"><div className="progress-fill" style={{width: '90%', background: '#3b82f6'}}></div></div>
              <div className="progress-text">18/20</div>
            </div>
            <div className="badge-card locked">
              <div className="badge-icon-lg lock">🎓</div>
              <h4>Chuyên gia an toàn</h4>
              <p>Hoàn thành tất cả chủ đề</p>
              <div className="progress-track mt-10"><div className="progress-fill" style={{width: '24%', background: '#8b5cf6'}}></div></div>
              <div className="progress-text">12/50</div>
            </div>
         </div>
      </section>

      <div className="achieve-2col-layout">
         <section className="mimo-panel-card p-24" style={{flex: 1.5}}>
            <div className="panel-header">
               <h2 className="text-blue flex-align-center gap-10"><span style={{fontSize:'20px'}}>📊</span> Hành trình phát triển</h2>
               <span className="badge-green-pill">Bạn đang ở đây!</span>
            </div>
            <div className="journey-timeline mt-24">
               <div className="timeline-line"></div>
               <div className="timeline-fill" style={{width: '55%'}}></div>
               <div className="timeline-points">
                 <div className="pt done">✓<br/><span>Cấp 1</span></div>
                 <div className="pt done">✓<br/><span>Cấp 2</span></div>
                 <div className="pt done">✓<br/><span>Cấp 3</span></div>
                 <div className="pt done">✓<br/><span>Cấp 4</span></div>
                 <div className="pt done">✓<br/><span>Cấp 5</span></div>
                 <div className="pt active">👑<br/><span>Cấp 6</span></div>
                 <div className="pt">7<br/><span>Cấp 7</span></div>
                 <div className="pt">8<br/><span>Cấp 8</span></div>
                 <div className="pt">9<br/><span>Cấp 9</span></div>
                 <div className="pt">10<br/><span>Cấp 10</span></div>
               </div>
            </div>
            <div className="flex-between mt-24 text-sm">
               <strong className="text-blue">3.250 / 3.500 XP</strong>
               <span className="text-blue font-bold">Còn 250 XP để lên cấp tiếp theo</span>
            </div>
         </section>

         <section className="mimo-panel-card p-24" style={{flex: 1}}>
            <div className="panel-header">
               <h2 className="text-blue flex-align-center gap-10"><span style={{fontSize:'20px'}}>🕒</span> Thành tích gần đây</h2>
               <a href="#" className="text-blue font-bold">Xem tất cả →</a>
            </div>
            <div className="recent-achieves-list mt-15">
               <div className="rc-item">
                  <div className="icon text-red bg-red-light">🚑</div>
                  <div className="rc-info">Hoàn thành bài học Sơ cứu cơ bản</div>
                  <div className="rc-xp">+100 XP</div>
                  <div className="rc-time">Hôm nay</div>
               </div>
               <div className="rc-item">
                  <div className="icon text-orange bg-orange-light">🔥</div>
                  <div className="rc-info">Đạt chuỗi 7 ngày học liên tục</div>
                  <div className="rc-xp">+200 XP</div>
                  <div className="rc-time">2 ngày trước</div>
               </div>
               <div className="rc-item">
                  <div className="icon text-green bg-green-light">🚦</div>
                  <div className="rc-info">Mở khóa huy hiệu Người tham gia an toàn</div>
                  <div className="rc-xp">+50 XP</div>
                  <div className="rc-time">4 ngày trước</div>
               </div>
               <div className="rc-item">
                  <div className="icon text-orange bg-orange-light">🔥</div>
                  <div className="rc-info">Hoàn thành tình huống Thoát hiểm khi có cháy</div>
                  <div className="rc-xp">+100 XP</div>
                  <div className="rc-time">6 ngày trước</div>
               </div>
               <div className="rc-item">
                  <div className="icon text-purple bg-purple-light">❤️</div>
                  <div className="rc-info">Hoàn thành bài học Lắng nghe và chia sẻ</div>
                  <div className="rc-xp">+50 XP</div>
                  <div className="rc-time">1 tuần trước</div>
               </div>
            </div>
         </section>
      </div>

      <section className="mimo-panel-card p-24">
         <div className="panel-header">
            <h2 className="text-blue flex-align-center gap-10"><span style={{fontSize:'20px'}}>🎯</span> Tiến độ theo chủ đề</h2>
         </div>
         <div className="topic-progress-grid mt-15">
            <div className="tp-card">
               <div className="tp-header">
                  <div className="icon bg-blue-light text-blue">🚦</div>
                  <h4>An toàn giao thông</h4>
               </div>
               <div className="progress-track"><div className="progress-fill" style={{width: '60%', background: '#3b82f6'}}></div></div>
               <div className="tp-footer">
                  <span className="text-blue bg-blue-light px-8 py-2 border-radius-4">📖 6/10 bài</span>
                  <span className="text-orange bg-orange-light px-8 py-2 border-radius-4">⭐ 2 huy hiệu</span>
               </div>
               <div className="tp-percent">60%</div>
            </div>
            <div className="tp-card">
               <div className="tp-header">
                  <div className="icon bg-orange-light text-orange">🔥</div>
                  <h4>Hỏa hoạn và Thoát hiểm</h4>
               </div>
               <div className="progress-track"><div className="progress-fill" style={{width: '37%', background: '#ea580c'}}></div></div>
               <div className="tp-footer">
                  <span className="text-orange bg-orange-light px-8 py-2 border-radius-4">📖 3/8 bài</span>
                  <span className="text-orange bg-orange-light px-8 py-2 border-radius-4">⭐ 1 huy hiệu</span>
               </div>
               <div className="tp-percent">37%</div>
            </div>
            <div className="tp-card">
               <div className="tp-header">
                  <div className="icon bg-green-light text-green">🚑</div>
                  <h4>Sơ cứu</h4>
               </div>
               <div className="progress-track"><div className="progress-fill" style={{width: '20%', background: '#10b981'}}></div></div>
               <div className="tp-footer">
                  <span className="text-green bg-green-light px-8 py-2 border-radius-4">📖 2/10 bài</span>
                  <span className="text-orange bg-orange-light px-8 py-2 border-radius-4">⭐ 1 huy hiệu</span>
               </div>
               <div className="tp-percent">20%</div>
            </div>
            <div className="tp-card">
               <div className="tp-header">
                  <div className="icon bg-purple-light text-purple">🛡️</div>
                  <h4>Bạo lực gia đình / học đường</h4>
               </div>
               <div className="progress-track"><div className="progress-fill" style={{width: '12%', background: '#8b5cf6'}}></div></div>
               <div className="tp-footer">
                  <span className="text-purple bg-purple-light px-8 py-2 border-radius-4">📖 1/8 bài</span>
                  <span className="text-orange bg-orange-light px-8 py-2 border-radius-4">⭐ 1 huy hiệu</span>
               </div>
               <div className="tp-percent">12%</div>
            </div>
         </div>
      </section>
    </>
  );

  const renderAchievementsRightPanel = () => (
    <>
      <div className="mimo-panel-card">
        <div className="panel-header">
          <h3 className="text-red flex-align-center gap-10">🎯 Mục tiêu tuần này</h3>
          <a href="#" className="text-blue font-bold">Sửa mục tiêu</a>
        </div>
        <div className="achieve-checklist">
          <div className="check-item-achieve done bg-green-light">
             <div className="box">✓</div>
             <div className="info">
                <h4>Đạt 300 XP trong tuần này</h4>
                <div className="progress-track mt-5"><div className="progress-fill" style={{width: '100%', background: '#10b981'}}></div></div>
                <div className="text-xs text-right mt-2 text-green font-bold">320 / 300 XP</div>
             </div>
             <div className="decor-icon text-green">✨</div>
          </div>
          <div className="check-item-achieve">
             <div className="box"></div>
             <div className="info">
                <h4>Hoàn thành 2 tình huống mô phỏng</h4>
                <div className="progress-track mt-5" style={{background: '#f1f5f9'}}><div className="progress-fill" style={{width: '50%', background: '#cbd5e1'}}></div></div>
                <div className="text-xs text-right mt-2 text-gray">1 / 2</div>
             </div>
          </div>
          <div className="check-item-achieve">
             <div className="box"></div>
             <div className="info">
                <h4>Giữ chuỗi học 7 ngày</h4>
                <div className="progress-track mt-5" style={{background: '#f1f5f9'}}><div className="progress-fill" style={{width: '42%', background: '#cbd5e1'}}></div></div>
                <div className="text-xs text-right mt-2 text-gray">3 / 7</div>
             </div>
          </div>
        </div>
      </div>

      <div className="mimo-panel-card">
        <div className="panel-header">
          <h3 className="text-orange flex-align-center gap-10">⭐ Huy hiệu sắp mở khóa</h3>
          <a href="#" className="text-blue font-bold">Xem tất cả →</a>
        </div>
        <div className="upcoming-badges mt-15">
           <div className="up-badge-item">
              <div className="icon text-red bg-red-light">🐾</div>
              <div className="info">
                 <h4>Người bạn động vật</h4>
                 <p>Hoàn thành 3 bài học đặc biệt về bảo vệ động vật</p>
                 <div className="progress-text">2 / 3</div>
              </div>
           </div>
           <div className="up-badge-item">
              <div className="icon text-blue bg-blue-light">🛡️</div>
              <div className="info">
                 <h4>Chiến binh dũng cảm</h4>
                 <p>Hoàn thành 10 tình huống thử thách</p>
                 <div className="progress-text">7 / 10</div>
              </div>
           </div>
           <div className="up-badge-item">
              <div className="icon text-green bg-green-light">🌍</div>
              <div className="info">
                 <h4>Vì một thế giới an toàn</h4>
                 <p>Hoàn thành tất cả bài học về môi trường và thiên tai</p>
                 <div className="progress-text">4 / 8</div>
              </div>
           </div>
        </div>
      </div>

      <div className="feedback-card achieve-promo flex-col text-center" style={{background: 'linear-gradient(135deg, #e0f2fe, #dbeafe)'}}>
         <h3 className="text-blue font-bold mb-10 text-lg">Bạn đang tiến bộ<br/>mỗi ngày!</h3>
         <div style={{fontSize: '80px', margin: '10px 0'}}>🤖⭐</div>
         <p className="text-sm text-blue mb-15">Kiến thức hôm nay giúp bạn an toàn hơn, tự tin hơn và tạo nên một thế giới tốt đẹp hơn!</p>
         <button className="btn-orange-gradient w-full">Tiếp tục cố gắng nhé! →</button>
      </div>
    </>
  );
