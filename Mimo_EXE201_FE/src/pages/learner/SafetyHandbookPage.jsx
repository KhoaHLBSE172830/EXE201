import React, { useState, useEffect } from 'react';
import { learnerService } from '../../services/modules/learnerService';
import { sound } from '../../utils/audioEffects';

export default function SafetyHandbookPage() {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedPillar, setSelectedPillar] = useState('all');
  const [loading, setLoading] = useState(true);
  const [hotlines, setHotlines] = useState([]);
  const [articles, setArticles] = useState([]);
  const [pillarsList, setPillarsList] = useState([]);
  const [selectedArticle, setSelectedArticle] = useState(null);

  useEffect(() => {
    const fetchHandbook = async () => {
      try {
        setLoading(true);
        const res = await learnerService.getHandbook(
          selectedPillar !== 'all' ? selectedPillar : null,
          searchTerm.trim() || null
        );
        const data = res?.data || res;
        setHotlines(data?.emergencyHotlines || [
          { name: 'Tổng đài Quốc gia Bảo vệ Trẻ em', dial: '111' },
          { name: 'Công an phản ứng nhanh', dial: '113' },
          { name: 'Cứu nạn cứu hộ & PCCC', dial: '114' },
          { name: 'Cấp cứu y tế', dial: '115' },
        ]);
        setArticles(data?.articles || []);
        if (data?.pillars && data.pillars.length > 0) {
          setPillarsList(['all', ...data.pillars]);
        }
      } catch (err) {
        console.error('Lỗi khi tải cẩm nang an toàn:', err);
      } finally {
        setLoading(false);
      }
    };

    const debounceTimer = setTimeout(() => {
      fetchHandbook();
    }, 250);

    return () => clearTimeout(debounceTimer);
  }, [selectedPillar, searchTerm]);

  const defaultPillars = [
    { id: 'all', name: 'Tất cả' },
    { id: 'Sơ cấp cứu', name: '🚑 Sơ cấp cứu' },
    { id: 'PCCC & Thoát hiểm', name: '🔥 PCCC & Thoát hiểm' },
    { id: 'An toàn giao thông', name: '🚦 An toàn giao thông' },
    { id: 'Phòng chống bạo lực', name: '🛡️ Phòng chống bạo lực' },
  ];

  const hotlineIcons = {
    '111': '🧒',
    '113': '👮',
    '114': '🚒',
    '115': '🚑',
  };

  const hotlineColors = {
    '111': '#7C3AED',
    '113': '#2563EB',
    '114': '#DC2626',
    '115': '#059669',
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '28px' }}>
      {/* 1. HEADER CẨM NANG HỌC TẬP */}
      <div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
          <span style={{ fontSize: '32px' }}>📚</span>
          <h1 style={{ fontSize: '26px', fontWeight: 900, color: '#0F172A', margin: 0 }}>
            Cẩm nang an toàn & Kỹ năng sinh tồn
          </h1>
        </div>
        <p style={{ color: '#64748B', fontSize: '14.5px', margin: '6px 0 0 0' }}>
          Thư viện quy chuẩn y tế, thoát hiểm khẩn cấp và căn cứ pháp lý gối đầu giường dành cho học sinh & phụ huynh.
        </p>
      </div>

      {/* 2. BẢNG GỌI KHẨN CẤP SOS 1-CHẠM (EMERGENCY HOTLINES) */}
      <div
        style={{
          background: '#FEF2F2',
          border: '2px solid #FECACA',
          borderRadius: '24px',
          padding: '24px',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '16px' }}>
          <span style={{ fontSize: '24px' }}>🚨</span>
          <div>
            <h3 style={{ fontSize: '18px', fontWeight: 800, color: '#991B1B', margin: 0 }}>
              Bảng Tổng Đài Khẩn Cấp Quốc Gia (Miễn Phí 24/7)
            </h3>
            <div style={{ fontSize: '12.5px', color: '#B91C1C' }}>
              Chạm để gọi ngay khi gặp nguy hiểm đến tính mạng, cháy nổ hoặc bạo lực
            </div>
          </div>
        </div>

        <div
          style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
            gap: '12px',
          }}
        >
          {hotlines.map((dial) => {
            const icon = hotlineIcons[dial.dial] || '📞';
            const color = hotlineColors[dial.dial] || '#DC2626';

            return (
              <a
                key={dial.dial}
                href={`tel:${dial.dial}`}
                onClick={() => sound.playClick()}
                style={{
                  textDecoration: 'none',
                  background: '#FFFFFF',
                  border: '2px solid #FCA5A5',
                  borderBottom: `4px solid ${color}`,
                  borderRadius: '16px',
                  padding: '14px 16px',
                  display: 'flex',
                  alignItems: 'center',
                  gap: '12px',
                  color: '#0F172A',
                  transition: 'transform 0.1s',
                }}
              >
                <div
                  style={{
                    width: '46px',
                    height: '46px',
                    borderRadius: '12px',
                    background: '#FEE2E2',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: '22px',
                    flexShrink: 0,
                  }}
                >
                  {icon}
                </div>
                <div>
                  <div style={{ fontSize: '22px', fontWeight: 900, color: color, lineHeight: 1 }}>
                    {dial.dial}
                  </div>
                  <div style={{ fontSize: '12px', fontWeight: 700, color: '#1E293B', marginTop: '4px' }}>
                    {dial.name}
                  </div>
                </div>
              </a>
            );
          })}
        </div>
      </div>

      {/* 3. THANH TÌM KIẾM NHANH & BỘ LỌC CHỦ ĐỀ */}
      <div style={{ display: 'flex', gap: '12px', flexWrap: 'wrap' }}>
        <div style={{ position: 'relative', flex: 1, minWidth: '240px' }}>
          <input
            type="text"
            placeholder="🔍 Tìm nhanh: bỏng, cháy chung cư, 114, chảy máu cam, bạo lực..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{
              width: '100%',
              padding: '14px 18px',
              borderRadius: '16px',
              border: '2px solid #E2E8F0',
              fontSize: '14.5px',
              fontFamily: 'inherit',
              outline: 'none',
              boxSizing: 'border-box',
              background: '#FFFFFF',
            }}
          />
        </div>

        {/* Filter Chips */}
        <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
          {defaultPillars.map((pil) => (
            <button
              key={pil.id}
              onClick={() => {
                sound.playClick();
                setSelectedPillar(pil.id);
              }}
              style={{
                padding: '10px 16px',
                borderRadius: '999px',
                border: '2px solid',
                borderColor: selectedPillar === pil.id ? '#22C55E' : '#E2E8F0',
                background: selectedPillar === pil.id ? '#DCFCE7' : '#FFFFFF',
                color: selectedPillar === pil.id ? '#15803D' : '#64748B',
                fontWeight: 700,
                fontSize: '13px',
                cursor: 'pointer',
              }}
            >
              {pil.name}
            </button>
          ))}
        </div>
      </div>

      {/* 4. DANH SÁCH BÀI CẨM NANG */}
      {loading ? (
        <div style={{ textAlign: 'center', padding: '60px 0', color: '#94A3B8', fontWeight: 700 }}>
          <div style={{ fontSize: '32px', marginBottom: '8px' }}>🔄</div>
          Đang tra cứu cơ sở dữ liệu cẩm nang an toàn sống...
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '18px' }}>
          {articles.length === 0 ? (
            <div style={{ textAlign: 'center', padding: '40px', color: '#94A3B8', fontWeight: 700 }}>
              Không tìm thấy bài viết nào phù hợp với từ khóa "{searchTerm}".
            </div>
          ) : (
            articles.map((art) => (
              <div
                key={art.id}
                style={{
                  background: '#FFFFFF',
                  border: '2px solid #E2E8F0',
                  borderRadius: '20px',
                  padding: '24px',
                  boxShadow: '0 4px 12px rgba(0,0,0,0.03)',
                  transition: 'border-color 0.2s',
                }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '12px', flexWrap: 'wrap', gap: '8px' }}>
                  <div style={{ display: 'flex', gap: '8px' }}>
                    <span
                      style={{
                        background: '#EFF6FF',
                        color: '#1E40AF',
                        fontSize: '11px',
                        fontWeight: 800,
                        padding: '4px 10px',
                        borderRadius: '999px',
                      }}
                    >
                      {art.pillar}
                    </span>
                    <span
                      style={{
                        background: '#FEF3C7',
                        color: '#B45309',
                        fontSize: '11px',
                        fontWeight: 800,
                        padding: '4px 10px',
                        borderRadius: '999px',
                      }}
                    >
                      {art.badge}
                    </span>
                  </div>

                  <span style={{ fontSize: '12.5px', color: '#64748B', fontWeight: 700 }}>
                    ⏱️ Thời gian đọc: {art.readTime || '3 phút'}
                  </span>
                </div>

                <h3 style={{ fontSize: '18.5px', fontWeight: 800, color: '#0F172A', margin: '0 0 10px 0' }}>
                  {art.title}
                </h3>

                {/* Nguyên tắc cốt lõi (Key Protocol) */}
                <div
                  style={{
                    background: '#F8FAFC',
                    borderLeft: '4px solid #3B82F6',
                    padding: '12px 16px',
                    borderRadius: '0 12px 12px 0',
                    marginBottom: '14px',
                    fontSize: '14px',
                    color: '#1E293B',
                    fontWeight: 600,
                    lineHeight: 1.5,
                  }}
                >
                  ⚡ <strong>Nguyên tắc vàng:</strong> {art.keyProtocol}
                </div>

                {/* Căn cứ văn bản [VB] hoặc chuyên môn [CM] */}
                <div
                  style={{
                    background: '#F0FDF4',
                    border: '1px solid #BBF7D0',
                    borderRadius: '12px',
                    padding: '10px 14px',
                    fontSize: '12.5px',
                    color: '#166534',
                    marginBottom: '16px',
                    fontWeight: 600,
                  }}
                >
                  📜 <strong>Cơ sở kiểm chứng:</strong> {art.legalBasis}
                </div>

                {/* Nút xem chi tiết / Thu gọn bài đọc */}
                <button
                  onClick={() => {
                    sound.playClick();
                    setSelectedArticle(selectedArticle?.id === art.id ? null : art);
                  }}
                  className="btn-3d btn-3d-blue btn-3d-sm"
                  style={{ fontSize: '12.5px', padding: '8px 18px' }}
                >
                  {selectedArticle?.id === art.id ? '▲ Thu gọn hướng dẫn' : '▼ Xem chi tiết quy trình'}
                </button>

                {/* Nội dung chi tiết mở rộng (Markdown format) */}
                {selectedArticle?.id === art.id && (
                  <div
                    style={{
                      marginTop: '18px',
                      paddingTop: '18px',
                      borderTop: '1.5px dashed #CBD5E1',
                      fontSize: '14px',
                      color: '#334155',
                      lineHeight: 1.7,
                      whiteSpace: 'pre-line',
                    }}
                  >
                    {art.contentMarkdown}
                  </div>
                )}
              </div>
            ))
          )}
        </div>
      )}
    </div>
  );
}
