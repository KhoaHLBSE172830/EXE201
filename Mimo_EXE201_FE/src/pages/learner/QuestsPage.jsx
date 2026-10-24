import React, { useState, useEffect } from 'react';
import { useOutletContext } from 'react-router-dom';
import { learnerService } from '../../services/modules/learnerService';
import { sound, fireConfetti } from '../../utils/audioEffects';

export default function QuestsPage() {
  const { refreshData } = useOutletContext();
  const [loading, setLoading] = useState(true);
  const [quests, setQuests] = useState([]);
  const [resetInSeconds, setResetInSeconds] = useState(0);
  const [claimingId, setClaimingId] = useState(null);
  const [toastMessage, setToastMessage] = useState(null);

  const fetchQuests = async () => {
    try {
      setLoading(true);
      const res = await learnerService.getDailyQuests();
      const questData = res?.data || res;
      setQuests(questData?.quests || []);
      setResetInSeconds(questData?.resetInSeconds || 0);
    } catch (err) {
      console.error('Lỗi khi tải nhiệm vụ:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchQuests();
  }, []);

  // Countdown timer for reset
  useEffect(() => {
    if (resetInSeconds <= 0) return;
    const interval = setInterval(() => {
      setResetInSeconds((prev) => (prev > 0 ? prev - 1 : 0));
    }, 1000);
    return () => clearInterval(interval);
  }, [resetInSeconds]);

  const formatCountdown = (seconds) => {
    const hrs = Math.floor(seconds / 3600);
    const mins = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    return `${hrs.toString().padStart(2, '0')}:${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  const handleClaimReward = async (questId) => {
    try {
      setClaimingId(questId);
      const res = await learnerService.claimDailyQuest(questId);
      const result = res?.data || res;

      sound.playVictory();
      fireConfetti();

      // Cập nhật local state
      setQuests((prev) =>
        prev.map((q) => (q.id === questId ? { ...q, isClaimed: true } : q))
      );

      setToastMessage(result?.message || '🎉 Nhận thưởng thành công!');
      setTimeout(() => setToastMessage(null), 4000);

      // Cập nhật lại số XP / Stats trên top header
      if (refreshData) {
        refreshData();
      }
    } catch (err) {
      console.error('Lỗi khi nhận thưởng:', err);
      alert(err.response?.data?.message || err.message || 'Không thể nhận thưởng lúc này.');
    } finally {
      setClaimingId(null);
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
          <span>🎁</span>
          <span>{toastMessage}</span>
        </div>
      )}

      {/* Header & Đồng hồ reset */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '16px' }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <span style={{ fontSize: '32px' }}>⚡</span>
            <h1 style={{ fontSize: '26px', fontWeight: 900, color: '#0F172A', margin: 0 }}>
              Nhiệm vụ rèn luyện hàng ngày
            </h1>
          </div>
          <p style={{ color: '#64748B', fontSize: '14.5px', margin: '6px 0 0 0' }}>
            Mỗi nhiệm vụ hoàn thành là một thói quen an toàn được củng cố. Tự động làm mới vào 00:00 mỗi ngày!
          </p>
        </div>

        {resetInSeconds > 0 && (
          <div
            style={{
              background: '#FEF3C7',
              border: '1.5px solid #FCD34D',
              padding: '8px 16px',
              borderRadius: '14px',
              display: 'flex',
              alignItems: 'center',
              gap: '8px',
              fontSize: '13px',
              fontWeight: 800,
              color: '#92400E',
            }}
          >
            <span>⏳ Làm mới sau:</span>
            <span style={{ fontFamily: 'monospace', fontSize: '14px', color: '#B45309' }}>
              {formatCountdown(resetInSeconds)}
            </span>
          </div>
        )}
      </div>

      {/* Danh sách nhiệm vụ ngày */}
      {loading ? (
        <div style={{ textAlign: 'center', padding: '60px 0', color: '#94A3B8', fontWeight: 700 }}>
          <div style={{ fontSize: '32px', marginBottom: '8px' }}>🔄</div>
          Đang đồng bộ tiến độ nhiệm vụ thực tế từ hệ thống...
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          {quests.map((q) => {
            const isDone = q.isCompleted || q.currentProgress >= q.target;
            const isClaimed = q.isClaimed;
            const rewardLabel = q.rewardType === 'XP'
              ? `+${q.rewardAmount} XP`
              : q.rewardType === 'Heart'
              ? `+${q.rewardAmount} Tim ❤️`
              : `+${q.rewardAmount} Bùa 🛡️`;

            return (
              <div
                key={q.id}
                style={{
                  background: '#FFFFFF',
                  borderRadius: '20px',
                  border: isDone && !isClaimed ? '2px solid #22C55E' : '2px solid #E2E8F0',
                  padding: '24px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  boxShadow: isDone && !isClaimed ? '0 6px 20px rgba(34, 197, 94, 0.12)' : '0 4px 12px rgba(0,0,0,0.03)',
                  transition: 'all 0.2s ease',
                }}
              >
                <div style={{ display: 'flex', alignItems: 'center', gap: '16px', flex: 1, maxWidth: '520px' }}>
                  <div
                    style={{
                      width: '56px',
                      height: '56px',
                      borderRadius: '16px',
                      background: isDone ? '#DCFCE7' : '#F1F5F9',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: '28px',
                      flexShrink: 0,
                    }}
                  >
                    {q.icon || '🎯'}
                  </div>

                  <div style={{ flex: 1 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '6px' }}>
                      <h3 style={{ fontSize: '16px', fontWeight: 800, color: '#0F172A', margin: 0 }}>
                        {q.title}
                      </h3>
                      <span style={{ fontSize: '13px', fontWeight: 800, color: '#B45309' }}>
                        {rewardLabel}
                      </span>
                    </div>

                    <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                      <div className="mimo-progress-bar" style={{ height: '12px', flex: 1, background: '#E2E8F0', borderRadius: '6px', overflow: 'hidden' }}>
                        <div
                          className="mimo-progress-fill"
                          style={{
                            height: '100%',
                            width: `${Math.min(100, (q.currentProgress / q.target) * 100)}%`,
                            background: isDone ? '#22C55E' : '#3B82F6',
                            borderRadius: '6px',
                            transition: 'width 0.4s ease',
                          }}
                        />
                      </div>
                      <span style={{ fontSize: '13px', fontWeight: 800, color: '#64748B', minWidth: '40px' }}>
                        {q.currentProgress}/{q.target}
                      </span>
                    </div>
                  </div>
                </div>

                <div style={{ marginLeft: '24px' }}>
                  {isDone && !isClaimed ? (
                    <button
                      onClick={() => handleClaimReward(q.id)}
                      disabled={claimingId === q.id}
                      className="btn-3d btn-3d-yellow btn-3d-sm"
                      style={{ cursor: claimingId === q.id ? 'wait' : 'pointer' }}
                    >
                      {claimingId === q.id ? 'Đang nhận...' : '🎁 NHẬN THƯỞNG'}
                    </button>
                  ) : isClaimed ? (
                    <span
                      style={{
                        background: '#DCFCE7',
                        color: '#15803D',
                        fontSize: '13px',
                        fontWeight: 800,
                        padding: '8px 16px',
                        borderRadius: '10px',
                        display: 'inline-flex',
                        alignItems: 'center',
                        gap: '4px',
                      }}
                    >
                      ✓ ĐÃ NHẬN
                    </span>
                  ) : (
                    <span
                      style={{
                        background: '#F8FAFC',
                        color: '#94A3B8',
                        fontSize: '13px',
                        fontWeight: 800,
                        padding: '8px 16px',
                        borderRadius: '10px',
                      }}
                    >
                      ĐANG HỌC
                    </span>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
