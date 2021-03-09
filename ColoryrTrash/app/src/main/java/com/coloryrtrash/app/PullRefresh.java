package com.coloryrtrash.app;

import android.content.Context;
import android.util.AttributeSet;
import android.view.MotionEvent;
import android.view.View;
import android.view.animation.Animation;
import android.view.animation.RotateAnimation;
import android.widget.*;

public class PullRefresh extends ListView implements AbsListView.OnScrollListener {

    private static final int DOWN_UPDATE = 111;
    private static final int PLAN_UPDATE = 112;
    private static final int PROCESS_UPDATE = 113;

    private int thisUpdateStatusValue = DOWN_UPDATE; // 默认一直是下拉刷新

    public PullRefresh(Context context, AttributeSet attrs) {
        super(context, attrs);
        setOnScrollListener(this);
        initHeader();
    }

    private View headerView;
    private int headerViewHeight;

    private ImageView ivHeaderArrow;
    private ProgressBar pbHeader;
    private TextView tvHeaderState;

    public PullRefresh(Context context) {
        super(context);
    }

    /**
     * 初始化头部 布局View相关
     */
    private void initHeader() {
        // 从布局中拿到一个View
        headerView = View.inflate(getContext(), R.layout.pull_lay, null);

        // 获取头部各个控件的值
        ivHeaderArrow = headerView.findViewById(R.id.pull_pre_img);
        pbHeader = headerView.findViewById(R.id.pull_next_img);
        tvHeaderState = headerView.findViewById(R.id.pull_tv);

        // 所以先测量后，就能得到测量后的高度了
        headerView.measure(0, 0); // 注意：传0系统会自动去测量View高度

        // 得到测量后的高度
        headerViewHeight = headerView.getMeasuredHeight();
        headerView.setPadding(0, -headerViewHeight, 0, 0);
        addHeaderView(headerView);
        initHeaderAnimation();
    }

    private RotateAnimation upRotateAnimation;
    private RotateAnimation downRotateAnimation;

    private void initHeaderAnimation() {
        upRotateAnimation = new RotateAnimation(
                0, 180,
                Animation.RELATIVE_TO_SELF, 0.5f,
                Animation.RELATIVE_TO_SELF, 0.5f);
        upRotateAnimation.setDuration(500);
        upRotateAnimation.setFillAfter(true);

        downRotateAnimation = new RotateAnimation(
                180, 360,
                Animation.RELATIVE_TO_SELF, 0.5f,
                Animation.RELATIVE_TO_SELF, 0.5f);
        downRotateAnimation.setDuration(500);
        downRotateAnimation.setFillAfter(true);
    }

    /**
     * 滑动的状态改变
     *
     * @param view
     * @param scrollState 有三种状态
     *                    SCROLL_STATE_IDLE 代表 滑动停止状态类似于手指松开UP
     *                    SCROLL_STATE_TOUCH_SCROLL 代表滑动触摸状态
     *                    SCROLL_STATE_FLING 快速滑动 猛的一滑
     */
    @Override
    public void onScrollStateChanged(AbsListView view, int scrollState) {
        // 如果是猛地滑动 或者 手指松开UP 才显示底部布局View
        if (scrollState == SCROLL_STATE_IDLE || scrollState == SCROLL_STATE_FLING) {
            // 判断必须是底部的Item的时候
            if (getLastVisiblePosition() == (getCount() - 1)) {
                // 回调接口方法
                if (null != customUpdateListViewBack) {
                    customUpdateListViewBack.refreshData();
                }
            }
        }
    }

    private int firstVisibleItem;

    /**
     * ListView滑动的监听方法
     *
     * @param view             当前ListView
     * @param firstVisibleItem 当前屏幕的第一个显示的Item
     * @param visibleItemCount 当前屏幕显示的Item数量
     * @param totalItemCount   总共Item数量
     */
    @Override
    public void onScroll(AbsListView view, int firstVisibleItem, int visibleItemCount, int totalItemCount) {
        this.firstVisibleItem = firstVisibleItem;
    }

    private int downY;

    @Override
    public boolean onTouchEvent(MotionEvent ev) {
        switch (ev.getAction()) {
            case MotionEvent.ACTION_DOWN:
                downY = (int) ev.getY();
                break;
            case MotionEvent.ACTION_UP:
                if (thisUpdateStatusValue == DOWN_UPDATE) {
                    headerView.setPadding(0, -headerViewHeight, 0, 0);
                } else {
                    headerView.setPadding(0, 0, 0, 0);
                    thisUpdateStatusValue = PROCESS_UPDATE;
                    updateHeaderState();
                }
                break;
            case MotionEvent.ACTION_MOVE:
                int cha = (int) ev.getY() - downY;
                if (this.firstVisibleItem == 0 && cha > 0) {
                    int paddingTop = -headerViewHeight + cha;
                    // Log.i(TAG, "paddingTop:" + paddingTop);

                    if (thisUpdateStatusValue == PROCESS_UPDATE) {
                        break;
                    }

                    if (paddingTop > 0 && thisUpdateStatusValue == DOWN_UPDATE) {
                        thisUpdateStatusValue = PLAN_UPDATE;

                        updateHeaderState();

                    } else if (paddingTop < 0 && thisUpdateStatusValue == PLAN_UPDATE) {
                        thisUpdateStatusValue = DOWN_UPDATE;

                        updateHeaderState();
                    }

                    headerView.setPadding(0, paddingTop, 0, 0);
                }
                break;
            default:
                break;
        }
        return super.onTouchEvent(ev); // 不返回ture 而是去调用父类的方法，是保证ListView自身的滑动功能正常
    }

    private void updateHeaderState() {

        switch (thisUpdateStatusValue) {
            case DOWN_UPDATE:
                ivHeaderArrow.startAnimation(downRotateAnimation);
                tvHeaderState.setText("下拉刷新");
                break;
            case PLAN_UPDATE:
                ivHeaderArrow.startAnimation(upRotateAnimation);
                tvHeaderState.setText("准备刷新");
                break;
            case PROCESS_UPDATE:
                ivHeaderArrow.setVisibility(INVISIBLE);
                ivHeaderArrow.clearAnimation();
                pbHeader.setVisibility(VISIBLE);
                tvHeaderState.setText("正在刷新中...");

                if (null != customUpdateListViewBack) {
                    customUpdateListViewBack.refreshData();
                }
                break;
            default:
                break;
        }
    }

    private OnRefreshListener customUpdateListViewBack;

    public void setCallback(OnRefreshListener back) {
        this.customUpdateListViewBack = back;
    }

    public void updateHeaderResult() {
        headerView.setPadding(0, -headerViewHeight, 0, 0);

        // 状态还原
        ivHeaderArrow.clearAnimation();
        tvHeaderState.setText("下拉刷新");

        ivHeaderArrow.setVisibility(VISIBLE);
        pbHeader.setVisibility(INVISIBLE);

        thisUpdateStatusValue = DOWN_UPDATE;
    }
}