import apiClient from '../api/apiClient';
import { API_ENDPOINTS } from '../api/endpoints';
import { mockSuccess, mockError } from '../mocks/mockHelper';
import { mockAccounts } from '../mocks/mockAuth';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

export const authService = {
  /**
   * Đăng nhập: Trả về { token, user }
   */
  async login(email, password) {
    if (USE_MOCK) {
      const cleanEmail = (email || '').trim().toLowerCase();
      const cleanPassword = (password || '').trim();

      // 1. Kiểm tra danh sách mockAccounts mặc định
      let found = mockAccounts.find(
        (acc) => acc.email.toLowerCase() === cleanEmail && acc.password === cleanPassword
      );

      // 2. Nếu không thấy, kiểm tra danh sách tài khoản người dùng tự đăng ký trong localStorage
      if (!found) {
        try {
          const registered = JSON.parse(localStorage.getItem('mimo_registered_users') || '[]');
          found = registered.find(
            (acc) => acc.email.toLowerCase() === cleanEmail && acc.password === cleanPassword
          );
        } catch {
          // Bỏ qua nếu parse JSON lỗi
        }
      }

      if (found) {
        return mockSuccess({
          token: `mock-token-${found.role.toLowerCase()}-${Date.now()}`,
          user: {
            id: found.id,
            name: found.name,
            email: found.email,
            role: found.role,
            avatar: found.avatar || 'https://api.dicebear.com/7.x/bottts/svg?seed=User',
          },
        });
      }
      return mockError('Email hoặc mật khẩu không đúng! (Gợi ý tài khoản mẫu: learner@mimo.com, educator@mimo.com, curator@mimo.com, admin@mimo.com / Mật khẩu: 123)', 401);
    }

    return apiClient.post(API_ENDPOINTS.AUTH.LOGIN, { email, password });
  },

  /**
   * Đăng ký
   */
  async register(formData) {
    if (USE_MOCK) {
      try {
        const registered = JSON.parse(localStorage.getItem('mimo_registered_users') || '[]');
        const newUser = {
          id: `user_custom_${Date.now()}`,
          email: formData.email.trim(),
          password: formData.password.trim(),
          name: formData.name || formData.email.split('@')[0],
          role: formData.role || 'LEARNER',
          avatar: `https://api.dicebear.com/7.x/bottts/svg?seed=${encodeURIComponent(formData.name || 'User')}`,
        };
        registered.push(newUser);
        localStorage.setItem('mimo_registered_users', JSON.stringify(registered));
      } catch (e) {
        console.error('Lỗi lưu tài khoản đăng ký:', e);
      }

      return mockSuccess({
        message: 'Đăng ký tài khoản thành công! Bạn có thể đăng nhập ngay bây giờ.',
      });
    }
    return apiClient.post(API_ENDPOINTS.AUTH.REGISTER, formData);
  },

  /**
   * Quên mật khẩu
   */
  async forgotPassword(email) {
    if (USE_MOCK) {
      return mockSuccess({
        message: 'Mã xác nhận OTP đã được gửi đến email của bạn.',
      });
    }
    return apiClient.post(API_ENDPOINTS.AUTH.FORGOT_PASSWORD, { email });
  },

  /**
   * Đăng xuất
   */
  logout() {
    localStorage.removeItem('mimo_token');
    localStorage.removeItem('mimo_user');
  },

  /**
   * Lấy user hiện tại từ localStorage
   */
  getCurrentUser() {
    try {
      const raw = localStorage.getItem('mimo_user');
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  },
};
