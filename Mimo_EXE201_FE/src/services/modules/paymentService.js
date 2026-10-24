import apiClient from '../api/apiClient';
import { API_ENDPOINTS } from '../api/endpoints';

export const paymentService = {
  /**
   * Tạo liên kết thanh toán VNPay Sandbox cho gói Pro hoặc Khóa học
   * @param {string} planCode Mã gói (PREMIUM_1M, PREMIUM_3M, PREMIUM_6M)
   * @param {string|null} courseId Mã khóa học nếu mua lẻ
   * @param {string|null} orderInfo Ghi chú đơn hàng
   * @returns {Promise<{ data: { paymentUrl: string, txnRef: string, amount: number, planCode: string } }>}
   */
  async createVnPayUrl(planCode, courseId = null, orderInfo = null) {
    const payload = {
      planCode,
      courseId,
      orderInfo,
    };
    const res = await apiClient.post(API_ENDPOINTS.PAYMENT.CREATE_VNPAY_URL, payload);
    return res;
  },

  /**
   * Xác thực và xử lý kết quả trả về từ cổng VNPay
   * @param {string} queryString Chuỗi query params dạng ?vnp_...
   * @returns {Promise<{ data: Object }>}
   */
  async verifyVnPayReturn(queryString) {
    const q = queryString.startsWith('?') ? queryString : `?${queryString}`;
    const separator = q.includes('?') ? '&' : '?';
    const res = await apiClient.get(`${API_ENDPOINTS.PAYMENT.VNPAY_RETURN}${q}${separator}json=true`);
    return res;
  },

  /**
   * Lấy lịch sử giao dịch thanh toán của người dùng hiện tại
   * @returns {Promise<{ data: Array }>}
   */
  async getPaymentHistory() {
    const res = await apiClient.get(API_ENDPOINTS.PAYMENT.HISTORY);
    return res;
  },

  /**
   * Kích hoạt giả lập gói Pro lưu thẳng vào Database (dành cho test/demo)
   * @param {string} planCode Mã gói (PREMIUM_1M, PREMIUM_3M, PREMIUM_6M)
   * @returns {Promise<{ data: Object }>}
   */
  async mockActivatePlan(planCode) {
    const res = await apiClient.post(API_ENDPOINTS.PAYMENT.MOCK_ACTIVATE, { planCode });
    return res;
  },
};
