import React, { useState, useEffect } from 'react';
import { useOutletContext } from 'react-router-dom';
import { learnerService } from '../../services/modules/learnerService';
import { sound, fireConfetti } from '../../utils/audioEffects';
import MimoPlusUpgradeModal from '../../components/modals/MimoPlusUpgradeModal';

export default function ShopRewardPage() {
  const { refreshData } = useOutletContext() || {};
  const [showPlusModal, setShowPlusModal] = useState(false);
  const [loading, setLoading] = useState(true);
  const [shopData, setShopData] = useState({
    totalXp: 0,
    hearts: 5,
    streakFreezeCount: 0,
    isPro: false,
    items: [],
  });
  const [purchasingId, setPurchasingId] = useState(null);
  const [toastMessage, setToastMessage] = useState(null);

  const fetchShop = async () => {
    try {
      setLoading(true);
      const res = await learnerService.getShopItems();
      const data = res?.data || res;
      setShopData({
        totalXp: data?.totalXp ?? 0,
        hearts: data?.hearts ?? 5,
        streakFreezeCount: data?.streakFreezeCount ?? 0,
        isPro: data?.isPro ?? false,
        items: data?.items || [],
      });
    } catch (err) {
      console.error('Lỗi khi tải cửa hàng:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchShop();
  }, []);

  const handlePurchase = async (item) => {
    if (item.id === 'pro_trial_7d') {
      setShowPlusModal(true);
      return;
    }

    if (shopData.totalXp < item.costXp) {
      alert(`Bạn cần thêm ${item.costXp - shopData.totalXp} XP nữa để đổi vật phẩm này.`);
      return;
    }

    try {
      setPurchasingId(item.id);
      const res = await learnerService.purchaseShopItem(item.id);
      const result = res?.data || res;

      sound.playVictory();
      fireConfetti();

      // Cập nhật lại số dư local
      setShopData((prev) => ({
        ...prev,
        totalXp: result?.newTotalXp ?? (prev.totalXp - item.costXp),
        hearts: result?.hearts ?? prev.hearts,
        streakFreezeCount: result?.streakFreezeCount ?? prev.streakFreezeCount,
        isPro: result?.isPro ?? prev.isPro,
      }));

      setToastMessage(result?.message || `Đã đổi thành công ${item.title}!`);
      setTimeout(() => setToastMessage(null), 4000);

      if (refreshData) {
        refreshData();
      }
    } catch (err) {
      console.error('Lỗi khi đổi vật phẩm:', err);
      alert(err.response?.data?.message || err.message || 'Không thể đổi vật phẩm lúc này.');
    } finally {
      setPurchasingId(null);
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '28px' }}>
      {/* Toast Alert */}
      {toastMessage && (
        <div
          style={{
            position: 'fixed',
            top: '24px',
            right: '24px',
            background: '#10B981',
            color: '#FFFFFF',
            padding: '14px 24px',
            borderRadius: '12px',
            boxShadow: '0 8px 24px rgba(16, 185, 129, 0.3)',
            zIndex: 9999,
            fontWeight: 800,
            fontSize: '15px',
            display: 'flex',
            alignItems: 'center',
            gap: '10px',
            animation: 'fadeIn 0.3s ease-out',
          }}
        >
          <span>🎉</span>
          <span>{toastMessage}</span>
        </div>
      )}

      {/* Header & Số dư ví XP */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '16px' }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <span style={{ fontSize: '32px' }}>💎</span>
            <h1 style={{ fontSize: '26px', fontWeight: 900, color: '#0F172A', margin: 0 }}>
              Cửa hàng Đổi thưởng & Vật phẩm
            </h1>
          </div>
          <p style={{ color: '#64748B', fontSize: '14.5px', margin: '6px 0 0 0' }}>
            Sử dụng điểm kinh nghiệm (XP) tích lũy để trang bị bùa hộ mệnh và hỗ trợ sinh tồn thực tế.
          </p>
        </div>

        {/* User Balance Chips */}
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px', flexWrap: 'wrap' }}>
          <div
            style={{
              background: '#FFFBEB',
              border: '1.5px solid #FCD34D',
              padding: '8px 16px',
              borderRadius: '14px',
              display: 'flex',
              alignItems: 'center',
              gap: '6px',
              fontWeight: 800,
              fontSize: '14px',
              color: '#B45309',
            }}
          >
            <span>⚡</span>
            <span>{shopData.totalXp} XP</span>
          </div>

          <div
            style={{
              background: '#FFE4E6',
              border: '1.5px solid #FDA4AF',
              padding: '8px 16px',
              borderRadius: '14px',
              display: 'flex',
              alignItems: 'center',
              gap: '6px',
              fontWeight: 800,
              fontSize: '14px',
              color: '#BE123C',
            }}
          >
            <span>❤️</span>
            <span>{shopData.isPro ? 'Vô hạn' : `${shopData.hearts}/5`}</span>
          </div>

          <div
            style={{
              background: '#E0F2FE',
              border: '1.5px solid #7DD3FC',
              padding: '8px 16px',
              borderRadius: '14px',
              display: 'flex',
              alignItems: 'center',
              gap: '6px',
              fontWeight: 800,
              fontSize: '14px',
              color: '#0369A1',
            }}
          >
            <span>🛡️</span>
            <span>{shopData.streakFreezeCount}/2 Bùa</span>
          </div>
        </div>
      </div>

      {/* BANNER MIMO PLUS CÁ NHÂN */}
      <div
        style={{
          background: 'linear-gradient(135deg, #7C3AED, #9333EA, #C026D3)',
          borderRadius: '24px',
          padding: '28px 32px',
          color: '#FFFFFF',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          gap: '20px',
          boxShadow: '0 12px 28px rgba(124, 58, 237, 0.25)',
        }}
      >
        <div style={{ maxWidth: '560px' }}>
          <div style={{ display: 'inline-flex', alignItems: 'center', gap: '6px', background: 'rgba(255, 255, 255, 0.2)', color: '#FFF', fontSize: '11px', fontWeight: 800, padding: '4px 10px', borderRadius: '999px', marginBottom: '8px' }}>
            <span>👑</span>
            <span>GÓI CÁ NHÂN CAO CẤP NHẤT</span>
          </div>

          <h2 style={{ fontSize: '24px', fontWeight: 900, margin: '4px 0 8px 0', letterSpacing: '-0.3px' }}>
            Nâng Cấp MIMO PLUS — Vô Hạn Tim ❤️
          </h2>

          <p style={{ fontSize: '14px', color: '#F5D0FE', margin: 0, lineHeight: 1.5 }}>
            Không bao giờ bị gián đoạn khi giải quyết tình huống khẩn cấp, mở khóa phân tích chuyên sâu của Bác sĩ y khoa & Chuyên gia PCCC, tự động bảo vệ chuỗi Streak và 100% không quảng cáo.
          </p>
        </div>

        <button
          onClick={() => {
            sound.playClick();
            setShowPlusModal(true);
          }}
          style={{
            background: '#FFFFFF',
            color: '#7C3AED',
            border: 'none',
            borderRadius: '16px',
            padding: '14px 28px',
            fontSize: '14.5px',
            fontWeight: 900,
            cursor: 'pointer',
            boxShadow: '0 8px 20px rgba(0, 0, 0, 0.15)',
            display: 'flex',
            alignItems: 'center',
            gap: '8px',
          }}
        >
          <span>✨</span>
          <span>NÂNG CẤP NGAY</span>
        </button>
      </div>

      {/* DANH SÁCH VẬT PHẨM QUY ĐỔI */}
      {loading ? (
        <div style={{ textAlign: 'center', padding: '60px 0', color: '#94A3B8', fontWeight: 700 }}>
          <div style={{ fontSize: '32px', marginBottom: '8px' }}>🔄</div>
          Đang tải danh mục vật phẩm từ hệ thống...
        </div>
      ) : (
        <div
          style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))',
            gap: '18px',
          }}
        >
          {shopData.items.map((item) => {
            const canAfford = shopData.totalXp >= item.costXp;
            return (
              <div
                key={item.id}
                style={{
                  background: '#FFFFFF',
                  border: '2px solid #E2E8F0',
                  borderRadius: '20px',
                  padding: '24px',
                  display: 'flex',
                  flexDirection: 'column',
                  justifyContent: 'space-between',
                  gap: '16px',
                  boxShadow: '0 4px 12px rgba(0, 0, 0, 0.03)',
                  transition: 'transform 0.2s ease',
                }}
              >
                <div style={{ display: 'flex', alignItems: 'flex-start', gap: '14px' }}>
                  <div
                    style={{
                      width: '52px',
                      height: '52px',
                      borderRadius: '16px',
                      background: item.bgColor || '#F1F5F9',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: '28px',
                      flexShrink: 0,
                    }}
                  >
                    {item.icon || '🛍️'}
                  </div>
                  <div>
                    <h3 style={{ fontSize: '16.5px', fontWeight: 800, color: '#0F172A', margin: '0 0 4px 0' }}>
                      {item.title}
                    </h3>
                    <p style={{ fontSize: '13px', color: '#64748B', margin: 0, lineHeight: 1.45 }}>
                      {item.description}
                    </p>
                  </div>
                </div>

                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', paddingTop: '12px', borderTop: '1px solid #F1F5F9' }}>
                  <span style={{ fontSize: '15px', fontWeight: 900, color: item.color || '#0284C7' }}>
                    {item.costXp > 0 ? `${item.costXp} XP` : 'Miễn phí'}
                  </span>

                  <button
                    onClick={() => handlePurchase(item)}
                    disabled={purchasingId === item.id || (!canAfford && item.costXp > 0)}
                    className="btn-3d btn-3d-blue btn-3d-sm"
                    style={{
                      padding: '8px 16px',
                      fontSize: '12.5px',
                      opacity: canAfford || item.costXp === 0 ? 1 : 0.5,
                      cursor: canAfford ? 'pointer' : 'not-allowed',
                    }}
                  >
                    {purchasingId === item.id ? 'Đang đổi...' : item.id === 'pro_trial_7d' ? 'NÂNG CẤP' : 'ĐỔI NGAY'}
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* GÓI DOANH NGHIỆP & NHÀ TRƯỜNG (B2B ENTERPRISE) */}
      <div
        style={{
          background: 'linear-gradient(135deg, #0F172A, #1E293B)',
          borderRadius: '24px',
          padding: '30px',
          color: '#FFFFFF',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          gap: '20px',
          boxShadow: '0 10px 25px rgba(15, 23, 42, 0.15)',
        }}
      >
        <div style={{ maxWidth: '580px' }}>
          <div style={{ display: 'inline-flex', alignItems: 'center', gap: '6px', background: '#0284C7', color: '#FFF', fontSize: '11px', fontWeight: 800, padding: '3px 10px', borderRadius: '999px', marginBottom: '8px' }}>
            <span>🏢</span>
            <span>DÀNH CHO NHÀ TRƯỜNG & DOANH NGHIỆP</span>
          </div>

          <h2 style={{ fontSize: '22px', fontWeight: 900, margin: '4px 0 8px 0', color: '#F8FAFC' }}>
            MIMO School & Enterprise — Đào Tạo An Toàn Tập Thể
          </h2>

          <p style={{ fontSize: '13.5px', color: '#94A3B8', margin: 0, lineHeight: 1.5 }}>
            Quản lý từ 15 đến 500+ học sinh/nhân viên, tự động phân khối lớp (10A1-12A5), cấp mã bản quyền giáo viên bộ môn, theo dõi lỗ hổng PCCC và xuất báo cáo kiểm định tuân thủ chính thức.
          </p>
        </div>

        <button
          onClick={() => {
            sound.playClick();
            window.location.href = '/business';
          }}
          style={{
            background: 'linear-gradient(135deg, #38BDF8, #0284C7)',
            color: '#FFFFFF',
            border: 'none',
            borderRadius: '14px',
            padding: '14px 26px',
            fontSize: '14px',
            fontWeight: 900,
            cursor: 'pointer',
            boxShadow: '0 6px 18px rgba(2, 132, 199, 0.4)',
            display: 'flex',
            alignItems: 'center',
            gap: '8px',
          }}
        >
          <span>🚀</span>
          <span>QUẢN TRỊ DOANH NGHIỆP / TRƯỜNG HỌC</span>
        </button>
      </div>

      {/* MODAL NÂNG CẤP MIMO PLUS */}
      <MimoPlusUpgradeModal
        isOpen={showPlusModal}
        onClose={() => setShowPlusModal(false)}
        onUpgradeSuccess={() => {
          fetchShop();
          if (refreshData) refreshData();
        }}
      />
    </div>
  );
}
