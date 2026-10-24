import apiClient from '../api/apiClient';
import { API_ENDPOINTS } from '../api/endpoints';
import { mockSuccess } from '../mocks/mockHelper';
import { mockAdminData } from '../mocks/mockAdmin';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

export const adminService = {
  // 1. Dashboard & Metrics
  async getAnalytics() {
    try {
      const res = await apiClient.get(API_ENDPOINTS.ADMIN.DASHBOARD);
      return res;
    } catch (err) {
      console.warn('API Admin Analytics fallback to mock:', err);
      return mockSuccess(mockAdminData.getMetrics());
    }
  },

  async getSkillGaps() {
    try {
      const res = await apiClient.get(API_ENDPOINTS.ADMIN.SKILL_GAPS);
      const items = Array.isArray(res?.data) ? res.data : (res?.data?.items || []);
      return { ...res, data: items };
    } catch (err) {
      console.warn('API Admin Skill Gaps fallback to mock:', err);
      return mockSuccess(mockAdminData.getSkillGaps());
    }
  },

  async broadcastSkillAlert(topicId, customMessage) {
    try {
      return await apiClient.post(API_ENDPOINTS.ADMIN.BROADCAST_ALERT, { topicId, customMessage });
    } catch (err) {
      console.warn('API Broadcast Alert fallback to mock:', err);
      return mockSuccess(mockAdminData.broadcastSkillAlert(topicId, customMessage));
    }
  },

  // 2. Content Approvals (Gatekeeper)
  async getPendingReviews() {
    try {
      const res = await apiClient.get(API_ENDPOINTS.ADMIN.APPROVALS);
      const items = Array.isArray(res?.data) ? res.data : (res?.data?.items || []);
      return { ...res, data: items };
    } catch (err) {
      console.warn('API Admin Reviews fallback to mock:', err);
      return mockSuccess(mockAdminData.getReviews());
    }
  },

  async approveReview(reviewId) {
    try {
      const res = await apiClient.post(API_ENDPOINTS.ADMIN.APPROVE_LESSON(reviewId));
      const items = Array.isArray(res?.data) ? res.data : (res?.data?.items || []);
      return { ...res, data: items };
    } catch (err) {
      console.warn('API Approve Review fallback to mock:', err);
      return mockSuccess(mockAdminData.approveReview(reviewId));
    }
  },

  async rejectReview(reviewId, reason) {
    try {
      const res = await apiClient.post(API_ENDPOINTS.ADMIN.REJECT_LESSON(reviewId), { reason });
      const items = Array.isArray(res?.data) ? res.data : (res?.data?.items || []);
      return { ...res, data: items };
    } catch (err) {
      console.warn('API Reject Review fallback to mock:', err);
      return mockSuccess(mockAdminData.rejectReview(reviewId, reason));
    }
  },

  async rollbackReview(reviewId) {
    try {
      const res = await apiClient.post(API_ENDPOINTS.ADMIN.ROLLBACK_LESSON(reviewId));
      const items = Array.isArray(res?.data) ? res.data : (res?.data?.items || []);
      return { ...res, data: items };
    } catch (err) {
      console.warn('API Rollback Review fallback to mock:', err);
      return mockSuccess(mockAdminData.rollbackReview(reviewId));
    }
  },

  async getCurators() {
    try {
      const res = await apiClient.get(API_ENDPOINTS.ADMIN.CURATORS);
      const items = Array.isArray(res?.data) ? res.data : (res?.data?.items || []);
      return { ...res, data: items };
    } catch (err) {
      console.warn('API Admin Curators fallback to mock:', err);
      return mockSuccess(mockAdminData.getCurators());
    }
  },

  // 3. Gamification Settings
  async getGamificationSettings() {
    try {
      return await apiClient.get(API_ENDPOINTS.ADMIN.SETTINGS);
    } catch (err) {
      console.warn('API Gamification Settings fallback to mock:', err);
      return mockSuccess(mockAdminData.getSettings());
    }
  },

  async saveGamificationSettings(settings) {
    try {
      return await apiClient.put(API_ENDPOINTS.ADMIN.SETTINGS, settings);
    } catch (err) {
      console.warn('API Save Gamification Settings fallback to mock:', err);
      return mockSuccess(mockAdminData.saveSettings(settings));
    }
  },

  async resetGamificationSettings() {
    try {
      return await apiClient.post(API_ENDPOINTS.ADMIN.SETTINGS_RESET);
    } catch (err) {
      console.warn('API Reset Gamification Settings fallback to mock:', err);
      return mockSuccess(mockAdminData.resetSettings());
    }
  },

  // 4. Users & CSKH Support
  async getUsers() {
    try {
      const res = await apiClient.get(API_ENDPOINTS.ADMIN.USERS);
      const items = Array.isArray(res?.data) ? res.data : (res?.data?.items || []);
      return { ...res, data: items };
    } catch (err) {
      console.warn('API Admin Users fallback to mock:', err);
      return mockSuccess(mockAdminData.getUsers());
    }
  },

  async rescueUserStreak(userId) {
    try {
      const res = await apiClient.post(API_ENDPOINTS.ADMIN.RESCUE_STREAK(userId), {});
      const items = Array.isArray(res?.data) ? res.data : (res?.data?.items || []);
      return { ...res, data: items };
    } catch (err) {
      console.warn('API Rescue Streak fallback to mock:', err);
      return mockSuccess(mockAdminData.rescueStreak(userId));
    }
  },

  async refillUserHearts(userId) {
    try {
      const res = await apiClient.post(API_ENDPOINTS.ADMIN.REFILL_HEARTS(userId));
      const items = Array.isArray(res?.data) ? res.data : (res?.data?.items || []);
      return { ...res, data: items };
    } catch (err) {
      console.warn('API Refill Hearts fallback to mock:', err);
      return mockSuccess(mockAdminData.refillHearts(userId));
    }
  },

  async rewardUserXP(userId, amount) {
    try {
      const res = await apiClient.post(API_ENDPOINTS.ADMIN.REWARD_XP(userId), { amount: Number(amount) });
      const items = Array.isArray(res?.data) ? res.data : (res?.data?.items || []);
      return { ...res, data: items };
    } catch (err) {
      console.warn('API Reward XP fallback to mock:', err);
      return mockSuccess(mockAdminData.rewardXP(userId, amount));
    }
  },

  async toggleUserBan(userId) {
    try {
      const res = await apiClient.post(API_ENDPOINTS.ADMIN.TOGGLE_BAN(userId));
      const items = Array.isArray(res?.data) ? res.data : (res?.data?.items || []);
      return { ...res, data: items };
    } catch (err) {
      console.warn('API Toggle Ban fallback to mock:', err);
      return mockSuccess(mockAdminData.toggleBanUser(userId));
    }
  },

  // 5. Schools B2B
  async getSchools() {
    try {
      const res = await apiClient.get(API_ENDPOINTS.ADMIN.SCHOOLS);
      const items = Array.isArray(res?.data) ? res.data : (res?.data?.items || []);
      return { ...res, data: items };
    } catch (err) {
      console.warn('API Admin Schools fallback to mock:', err);
      return mockSuccess(mockAdminData.getSchools());
    }
  },

  async createSchoolContract(schoolData) {
    try {
      const res = await apiClient.post(API_ENDPOINTS.ADMIN.SCHOOLS, schoolData);
      const items = Array.isArray(res?.data) ? res.data : (res?.data?.items || []);
      return { ...res, data: items };
    } catch (err) {
      console.warn('API Create School Contract fallback to mock:', err);
      return mockSuccess(mockAdminData.createSchool(schoolData));
    }
  },

  async generateClassCodes(schoolId, count) {
    try {
      const res = await apiClient.post(API_ENDPOINTS.ADMIN.GENERATE_CLASSES(schoolId), { count: Number(count) });
      const items = Array.isArray(res?.data) ? res.data : (res?.data?.items || []);
      return { ...res, data: items };
    } catch (err) {
      console.warn('API Generate Class Codes fallback to mock:', err);
      return mockSuccess(mockAdminData.generateClassCodes(schoolId, count));
    }
  },

  // 6. Feedback & Tickets
  async getFeedbacks() {
    try {
      const res = await apiClient.get(API_ENDPOINTS.ADMIN.FEEDBACKS);
      const items = Array.isArray(res?.data) ? res.data : (res?.data?.items || []);
      return { ...res, data: items };
    } catch (err) {
      console.warn('API Admin Feedbacks fallback to mock:', err);
      return mockSuccess(mockAdminData.getFeedbacks());
    }
  },

  async resolveFeedback(feedbackId, xpGift) {
    try {
      const res = await apiClient.post(API_ENDPOINTS.ADMIN.RESOLVE_FEEDBACK(feedbackId), { xpGift: Number(xpGift) || 0 });
      const items = Array.isArray(res?.data) ? res.data : (res?.data?.items || []);
      return { ...res, data: items };
    } catch (err) {
      console.warn('API Resolve Feedback fallback to mock:', err);
      return mockSuccess(mockAdminData.resolveFeedback(feedbackId, xpGift));
    }
  },

  // 7. Monetization & Billing
  async getMonetizationData() {
    try {
      return await apiClient.get(API_ENDPOINTS.ADMIN.MONETIZATION);
    } catch (err) {
      console.warn('API Admin Monetization fallback to mock:', err);
      return mockSuccess(mockAdminData.getMonetization());
    }
  },

  async refundTransaction(transactionId) {
    try {
      return await apiClient.post(API_ENDPOINTS.ADMIN.REFUND(transactionId));
    } catch (err) {
      console.warn('API Refund Transaction fallback to mock:', err);
      return mockSuccess({ success: true, message: `Đã hoàn tiền giao dịch ${transactionId}` });
    }
  },
};
